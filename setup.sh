#!/bin/bash
set -e

# Configuration
RG_NAME="SchoolERP-RG"
LOCATION="centralindia"
DB_PASS="SiVa@30$"

echo "========================================"
echo " Starting SchoolERP Environment Setup"
echo "========================================"

# 1. Create Resource Group
echo "Creating Resource Group: $RG_NAME in $LOCATION..."
az group create --name $RG_NAME --location $LOCATION -o none

# 2. Get User Object ID
echo "Fetching signed-in user object ID..."
USER_OID=$(az ad signed-in-user show --query id -o tsv)
echo "User Object ID: $USER_OID"

# 3. Purge Soft-Deleted Key Vaults
echo "Checking for soft-deleted Key Vaults to purge (to avoid ConflictErrors)..."
DELETED_KVS=$(az keyvault list-deleted --query "[?starts_with(name, 'kverp')].name" -o tsv 2>/dev/null || echo "")
for kv in $DELETED_KVS; do
  if [ -n "$kv" ]; then
    echo "Purging deleted Key Vault: $kv..."
    az keyvault purge --name "$kv" -o none
    # Wait a few seconds for the purge to complete on Azure's backend
    sleep 20
  fi
done

# 4. Deploy Bicep Template
echo "Deploying infrastructure using Bicep..."
DEPLOYMENT_OUTPUT=$(az deployment group create \
  --resource-group $RG_NAME \
  --template-file main.bicep \
  --parameters dbAdminPassword="$DB_PASS" userObjectId="$USER_OID" \
  --output json)

# Extract outputs
ACR_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.acrLoginServer.value' | cut -d'.' -f1)
AKS_NAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.aksClusterName.value')
PG_SERVER=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.postgresServerName.value')
SWA_HOSTNAME=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.staticWebAppDefaultHostName.value')
KEY_VAULT_URI=$(echo $DEPLOYMENT_OUTPUT | jq -r '.properties.outputs.keyVaultUri.value')

echo "Pushing FrontendUrl to Key Vault..."
KV_NAME_TEMP=$(az keyvault list --query "[?starts_with(name, 'kverp')].name" -o tsv | head -n 1)
az keyvault secret set --vault-name $KV_NAME_TEMP --name "FrontendUrl" --value "https://$SWA_HOSTNAME" > /dev/null

echo "✅ Azure Infrastructure Deployment Complete!"

UNIQUE_ID=$(echo $AKS_NAME | awk -F'-' '{print $3}')
SWA_NAME="swaschoolerp${UNIQUE_ID}"
FUNC_APP_NAME="func-schoolerp-${UNIQUE_ID}"
KV_NAME=$(echo $KEY_VAULT_URI | awk -F'/' '{print $3}' | cut -d'.' -f1)

echo "ACR Name: $ACR_NAME"
echo "AKS Name: $AKS_NAME"
echo "Postgres Server: $PG_SERVER"
echo "Static Web App: $SWA_NAME"
echo "Function App: $FUNC_APP_NAME"
echo "Key Vault: $KV_NAME"

# 4. Inject Secrets into Azure Key Vault
echo "Extracting local dotnet user-secrets..."
STRIPE_SECRET=$(cd SchoolERPManagementAPI && dotnet user-secrets list | grep 'Stripe:SecretKey' | cut -d'=' -f2 | xargs)
STRIPE_WEBHOOK=$(cd SchoolERPManagementAPI && dotnet user-secrets list | grep 'Stripe:WebhookSecret' | cut -d'=' -f2 | xargs)
SMTP_PASSWORD=$(cd SchoolERPManagementAPI && dotnet user-secrets list | grep 'SmtpSettings:Password' | cut -d'=' -f2 | xargs)

if [ -n "$STRIPE_SECRET" ]; then
  echo "Pushing Stripe--SecretKey to Azure Key Vault..."
  az keyvault secret set --vault-name $KV_NAME --name "Stripe--SecretKey" --value "$STRIPE_SECRET" -o none
fi
if [ -n "$STRIPE_WEBHOOK" ]; then
  echo "Pushing Stripe--WebhookSecret to Azure Key Vault..."
  az keyvault secret set --vault-name $KV_NAME --name "Stripe--WebhookSecret" --value "$STRIPE_WEBHOOK" -o none
fi
if [ -n "$SMTP_PASSWORD" ]; then
  echo "Pushing SmtpSettings--Password to Azure Key Vault..."
  az keyvault secret set --vault-name $KV_NAME --name "SmtpSettings--Password" --value "$SMTP_PASSWORD" -o none
fi

# Generate and push a secure JWT Key
echo "Generating secure JWT Key and pushing to Key Vault..."
JWT_KEY=$(openssl rand -base64 32 | tr -d '\n')
az keyvault secret set --vault-name $KV_NAME --name "Jwt--Key" --value "$JWT_KEY" -o none

# 5. Fetch ACR Credentials
echo "Fetching ACR credentials..."
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query "username" -o tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)

# Get Kubeconfig for AKS
rm -f kubeconfig
az aks get-credentials -n $AKS_NAME -g $RG_NAME --file kubeconfig
export KUBECONFIG=kubeconfig

# 6. Create app-config ConfigMap
echo "Creating app-config ConfigMap in Kubernetes..."
kubectl create configmap app-config \
  --from-literal=KeyVaultUri="$KEY_VAULT_URI" \
  --from-literal=StripeSuccessUrl="https://$SWA_HOSTNAME/payment-success" \
  --from-literal=StripeCancelUrl="https://$SWA_HOSTNAME/payment-cancelled" \
  --dry-run=client -o yaml | kubectl apply -f -

# 7. Create ACR Pull Secret in Kubernetes
echo "Creating acr-auth secret in Kubernetes..."
kubectl create secret docker-registry acr-auth \
  --docker-server="$ACR_LOGIN_SERVER" \
  --docker-username="$ACR_USERNAME" \
  --docker-password="$ACR_PASSWORD" \
  --docker-email="admin@schoolerp.com" --dry-run=client -o yaml | kubectl apply -f -

# Create AI Backend Secrets
echo "Creating ai-backend-secrets in Kubernetes..."
DB_CONN=$(az keyvault secret show --vault-name $KV_NAME --name "ConnectionStrings--Default" --query value -o tsv)
JWT_SECRET_VALUE=$(az keyvault secret show --vault-name $KV_NAME --name "Jwt--Key" --query value -o tsv)

READONLY_PASS='SchoolERP_AI_ReadOnly_2026$!'
URL_ENC_PASS=$(echo -n "$READONLY_PASS" | jq -sRr @uri)
PYTHON_DB_CONN="postgresql+psycopg2://readonly_ai_user:${URL_ENC_PASS}@${PG_SERVER}.postgres.database.azure.com:5432/SchoolERPSystem?sslmode=require"

# Fetch Anthropic API Key from local .env file
ANTHROPIC_KEY=$(grep '^ANTHROPIC_API_KEY=' school-erp-ai-backend/.env | cut -d '=' -f2- | tr -d '"' | xargs)
ANTHROPIC_URL=$(grep '^ANTHROPIC_BASE_URL=' school-erp-ai-backend/.env | cut -d '=' -f2- | tr -d '"' | xargs)
if [ -z "$ANTHROPIC_URL" ]; then
  ANTHROPIC_URL="https://proxy.llm-gateway.ready.presidio.com"
fi
kubectl create secret generic ai-backend-secrets \
  --from-literal=DB_CONNECTION_STRING="$PYTHON_DB_CONN" \
  --from-literal=JWT_SECRET="$JWT_SECRET_VALUE" \
  --from-literal=ANTHROPIC_API_KEY="$ANTHROPIC_KEY" \
  --from-literal=ANTHROPIC_BASE_URL="$ANTHROPIC_URL" \
  --dry-run=client -o yaml | kubectl apply -f -

# 8. Configure GitHub Secrets
echo "Configuring GitHub Secrets..."
gh secret set ACR_LOGIN_SERVER --body "$ACR_LOGIN_SERVER"
gh secret set ACR_USERNAME --body "$ACR_USERNAME"
gh secret set ACR_PASSWORD --body "$ACR_PASSWORD"
gh secret set KUBE_CONFIG < kubeconfig

# Get SWA Token and set secret
SWA_TOKEN=$(az staticwebapp secrets list --name $SWA_NAME --resource-group $RG_NAME --query "properties.apiKey" -o tsv)
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$SWA_TOKEN"

# 9. Install NGINX Ingress and Cert-Manager
echo "Installing NGINX Ingress Controller..."
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.8.2/deploy/static/provider/cloud/deploy.yaml
echo "Installing Cert-Manager..."
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.2/cert-manager.yaml
echo "Waiting for Cert-Manager webhooks to be ready..."
sleep 30

# 10. Trigger Backend CI/CDs
echo "Triggering .NET backend CI/CD workflow..."
gh workflow run backend-ci-cd.yml
echo "Triggering AI backend CI/CD workflow..."
gh workflow run ai-backend-ci-cd.yml
echo "Waiting 60 seconds for GitHub Actions to deploy the backends..."
sleep 60

# 10. Wait for Ingress LoadBalancer IP
echo "Waiting for ingress-nginx-controller to obtain a Public IP..."
export KUBECONFIG=kubeconfig
EXTERNAL_IP=""
while [ -z "$EXTERNAL_IP" ] || [ "$EXTERNAL_IP" == "<pending>" ] || [ "$EXTERNAL_IP" == "pending" ]; do
  sleep 10
  EXTERNAL_IP=$(kubectl get svc ingress-nginx-controller -n ingress-nginx -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
  echo "Current IP Status: ${EXTERNAL_IP:-Pending...}"
done

echo "Assigned Public IP: $EXTERNAL_IP"

# 11. Assign Azure DNS Label
echo "Assigning DNS label to Public IP..."
PIP_NAME=$(az network public-ip list -g MC_${RG_NAME}_${AKS_NAME}_${LOCATION} --query "[?ipAddress=='$EXTERNAL_IP'].name" -o tsv)
az network public-ip update -g MC_${RG_NAME}_${AKS_NAME}_${LOCATION} -n $PIP_NAME --dns-name "schoolerp-api-$UNIQUE_ID" -o none
API_DOMAIN="schoolerp-api-$UNIQUE_ID.$LOCATION.cloudapp.azure.com"
echo "API Domain is: https://$API_DOMAIN"

# 12. Apply Ingress Resource
echo "Applying Ingress and Cert-Manager ClusterIssuer..."
sed "s/API_DOMAIN_PLACEHOLDER/$API_DOMAIN/g" k8s/backend-ingress.yaml | kubectl apply -f -

# 13. Update DB Firewall with new IP
echo "Updating PostgreSQL Firewall to allow AKS IP..."
az postgres flexible-server firewall-rule create \
  --resource-group $RG_NAME \
  --server-name $PG_SERVER \
  --name AllowAKS \
  --start-ip-address $EXTERNAL_IP \
  --end-ip-address $EXTERNAL_IP -o none

# 14. Update Frontend Code
echo "Updating environment.prod.ts with new HTTPS backend URL..."
cat <<EOF > SchoolERPManagement.Frontend/src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'https://$API_DOMAIN/api',
  baseUrl: 'https://$API_DOMAIN',
  hubUrl: 'https://$API_DOMAIN/hubs',
  aiApiUrl: 'https://$API_DOMAIN'
};
EOF

# 15. Commit and Push Frontend Updates
echo "Committing and pushing frontend updates..."
git add SchoolERPManagement.Frontend/src/environments/environment.prod.ts
git commit -m "Automated update of backend IP to HTTPS Domain $API_DOMAIN" || echo "No changes to commit"
git push origin main

# 16. Deploy Azure Functions
echo "Configuring Azure Function App settings..."
az functionapp config appsettings set \
  --name $FUNC_APP_NAME \
  --resource-group $RG_NAME \
  --settings "API_URL=https://$API_DOMAIN" -o none

echo "Deploying Azure Functions..."
cd SchoolERPManagement.Functions
func azure functionapp publish $FUNC_APP_NAME
cd ..

echo "========================================"
echo " Setup Complete!"
echo " Frontend URL: https://$SWA_HOSTNAME"
echo " Backend API: https://$API_DOMAIN/api"
echo " GitHub Actions is currently building and deploying your Frontend!"
echo "========================================"
