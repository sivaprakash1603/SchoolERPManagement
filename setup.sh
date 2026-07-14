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

# 3. Deploy Bicep Template
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

UNIQUE_ID=$(echo $AKS_NAME | awk -F'-' '{print $3}')
SWA_NAME="swaschoolerp${UNIQUE_ID}"
FUNC_APP_NAME="func-schoolerp-${UNIQUE_ID}"

echo "ACR Name: $ACR_NAME"
echo "AKS Name: $AKS_NAME"
echo "Postgres Server: $PG_SERVER"
echo "Static Web App: $SWA_NAME"
echo "Function App: $FUNC_APP_NAME"

# 4. Attach ACR to AKS
echo "Attaching ACR to AKS (this may take a few minutes)..."
az aks update -n $AKS_NAME -g $RG_NAME --attach-acr $ACR_NAME -o none

# 5. Configure GitHub Secrets
echo "Fetching credentials and configuring GitHub Secrets..."
ACR_LOGIN_SERVER=$(az acr show --name $ACR_NAME --query loginServer --output tsv)
ACR_USERNAME=$(az acr credential show --name $ACR_NAME --query "username" -o tsv)
ACR_PASSWORD=$(az acr credential show --name $ACR_NAME --query "passwords[0].value" -o tsv)

gh secret set ACR_LOGIN_SERVER --body "$ACR_LOGIN_SERVER"
gh secret set ACR_USERNAME --body "$ACR_USERNAME"
gh secret set ACR_PASSWORD --body "$ACR_PASSWORD"

# Get Kubeconfig
rm -f kubeconfig
az aks get-credentials -n $AKS_NAME -g $RG_NAME --file kubeconfig
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
  --name $PG_SERVER \
  --rule-name AllowAKS \
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
