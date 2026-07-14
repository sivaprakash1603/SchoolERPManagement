@description('Location for all resources.')
param location string = resourceGroup().location

@description('The object ID of the user deploying this template, to grant Key Vault access.')
param userObjectId string

@description('Name of the Key Vault')
param keyVaultName string = 'kverp${substring(uniqueString(resourceGroup().id), 0, 8)}'

@description('Name of the Storage Account')
param storageAccountName string = 'stschoolerp${uniqueString(resourceGroup().id)}'

@description('Name of the blob container for uploads')
param containerName string = 'school-erp-uploads'

@description('Name of the Static Web App for the Angular frontend')
param staticWebAppName string = 'swaschoolerp${uniqueString(resourceGroup().id)}'

@description('Name of the Azure Container Registry')
param acrName string = 'acrerp${uniqueString(resourceGroup().id)}'

@description('Name of the Azure Kubernetes Service cluster')
param aksName string = 'aks-schoolerp-${uniqueString(resourceGroup().id)}'

// 1. Create the Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: true
    minimumTlsVersion: 'TLS1_2'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: containerName
  properties: {
    publicAccess: 'Blob'
  }
}

// 2. Create the Key Vault with Access Policies
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: userObjectId
        permissions: {
          secrets: [ 'get', 'list', 'set', 'delete', 'recover', 'backup', 'restore' ]
        }
      }
    ]
    enableRbacAuthorization: false
  }
}

var storageAccountKey = storageAccount.listKeys().keys[0].value
var connectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccountKey};EndpointSuffix=${environment().suffixes.storage}'

resource secret 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'AzureBlobStorage--ConnectionString'
  properties: {
    value: connectionString
  }
}

// 3. Create the Azure Static Web App
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: staticWebAppName
  location: 'eastasia' // centralindia is not supported
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// 4. Create Azure Container Registry (ACR)
// Admin user is enabled so Contributor can use the credentials in Kubernetes (since Contributors can't assign RBAC roles for Managed Identities)
resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// 5. Create Azure Kubernetes Service (AKS)
resource aks 'Microsoft.ContainerService/managedClusters@2024-02-01' = {
  name: aksName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: 'aks-schoolerp'
    agentPoolProfiles: [
      {
        name: 'agentpool'
        count: 1
        vmSize: 'Standard_D2as_v5'
        osType: 'Linux'
        mode: 'System'
      }
    ]
  }
}

output keyVaultUri string = keyVault.properties.vaultUri
output storageAccountName string = storageAccount.name
output containerName string = blobContainer.name
output staticWebAppDefaultHostName string = staticWebApp.properties.defaultHostname
output acrLoginServer string = acr.properties.loginServer
output aksClusterName string = aks.name
