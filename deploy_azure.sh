#!/bin/bash

# Configuration
RESOURCE_GROUP="rg-schoolerp"
LOCATION="eastus" # Change this if you prefer a different region

# Ensure you are logged in
echo "Checking Azure login status..."
az account show > /dev/null
if [ $? -ne 0 ]; then
    echo "You are not logged into Azure CLI. Please run 'az login' first."
    exit 1
fi

echo "Creating Resource Group: $RESOURCE_GROUP in $LOCATION..."
az group create --name $RESOURCE_GROUP --location $LOCATION

# Get the current logged-in user's Object ID
echo "Retrieving your Azure AD Object ID..."
USER_OBJECT_ID=$(az ad signed-in-user show --query id -o tsv)

if [ -z "$USER_OBJECT_ID" ]; then
    echo "Failed to retrieve your Object ID. Make sure your account has permissions."
    exit 1
fi

echo "Deploying Azure Infrastructure via Bicep..."
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file main.bicep \
  --parameters userObjectId=$USER_OBJECT_ID

echo "Deployment complete. Check the output above for your Key Vault URL."
