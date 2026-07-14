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

# 6. Configure GitHub Secrets
echo "Configuring GitHub Secrets..."
gh secret set ACR_LOGIN_SERVER --body "$ACR_LOGIN_SERVER"
gh secret set ACR_USERNAME --body "$ACR_USERNAME"
gh secret set ACR_PASSWORD --body "$ACR_PASSWORD"
gh secret set KUBE_CONFIG < kubeconfig

# Get SWA Token and set secret
SWA_TOKEN=$(az staticwebapp secrets list --name $SWA_NAME --resource-group $RG_NAME --query "properties.apiKey" -o tsv)
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$SWA_TOKEN"

# 6. Trigger Backend CI/CD
echo "Triggering backend CI/CD workflow..."
gh workflow run backend-ci-cd.yml
echo "Waiting 60 seconds for GitHub Actions to deploy the backend..."
sleep 60

# 7. Wait for AKS LoadBalancer IP
echo "Waiting for backend-api-service to obtain a Public IP (Load Balancer)..."
export KUBECONFIG=kubeconfig
EXTERNAL_IP=""
while [ -z "$EXTERNAL_IP" ] || [ "$EXTERNAL_IP" == "<pending>" ] || [ "$EXTERNAL_IP" == "pending" ]; do
  sleep 10
  EXTERNAL_IP=$(kubectl get svc backend-api-service -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
  echo "Current IP Status: ${EXTERNAL_IP:-Pending...}"
done

echo "Assigned Public IP: $EXTERNAL_IP"

# 8. Update DB Firewall with new IP
echo "Updating PostgreSQL Firewall to allow AKS IP..."
az postgres flexible-server firewall-rule create \
  --resource-group $RG_NAME \
  --server-name $PG_SERVER \
  --name AllowAKS \
  --start-ip-address $EXTERNAL_IP \
  --end-ip-address $EXTERNAL_IP -o none

# 9. Update Frontend Code
echo "Updating environment.prod.ts with new backend IP..."
cat <<EOF > SchoolERPManagement.Frontend/src/environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: 'http://$EXTERNAL_IP/api',
  baseUrl: 'http://$EXTERNAL_IP',
  hubUrl: 'http://$EXTERNAL_IP/hubs',
};
EOF

# 10. Commit and Push Frontend Updates
echo "Committing and pushing frontend updates..."
git add SchoolERPManagement.Frontend/src/environments/environment.prod.ts
git commit -m "Automated update of backend IP to $EXTERNAL_IP" || echo "No changes to commit"
git push origin main

# 11. Deploy Azure Functions
echo "Configuring Azure Function App settings..."
az functionapp config appsettings set \
  --name $FUNC_APP_NAME \
  --resource-group $RG_NAME \
  --settings "API_URL=http://$EXTERNAL_IP" -o none

echo "Deploying Azure Functions..."
cd SchoolERPManagement.Functions
func azure functionapp publish $FUNC_APP_NAME
cd ..

echo "========================================"
echo " Setup Complete!"
echo " Frontend URL: https://$SWA_HOSTNAME"
echo " Backend API: http://$EXTERNAL_IP/api"
echo " GitHub Actions is currently building and deploying your Frontend!"
echo "========================================"
