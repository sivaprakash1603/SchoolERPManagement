@description('Location for all resources.')
param location string = resourceGroup().location

@description('The object ID of the user deploying this template, to grant Key Vault access.')
param userObjectId string

@description('Administrator password for the PostgreSQL Database.')
@secure()
param dbAdminPassword string

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

@description('Name of the PostgreSQL Flexible Server')
param postgresServerName string = 'pg-schoolerp-${uniqueString(resourceGroup().id)}'

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

// 2. Create Azure Kubernetes Service (AKS)
// We declare AKS before Key Vault so we can grab its principalId for the access policy!
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

// 3. Create the Key Vault with Access Policies
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
      {
        tenantId: subscription().tenantId
        objectId: aks.properties.identityProfile.kubeletidentity.objectId // Grant AKS kubelet read access
        permissions: {
          secrets: [ 'get', 'list' ]
        }
      }
    ]
    enableRbacAuthorization: false
  }
}

var storageAccountKey = storageAccount.listKeys().keys[0].value
var blobConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccountKey};EndpointSuffix=${environment().suffixes.storage}'

resource secretStorage 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'AzureBlobStorage--ConnectionString'
  properties: {
    value: blobConnectionString
  }
}

// 4. Create the Azure Static Web App
resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: staticWebAppName
  location: 'eastasia' // centralindia is not supported
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// 5. Create Azure Container Registry (ACR)
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

// 6. Create PostgreSQL Flexible Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2022-12-01' = {
  name: postgresServerName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '15'
    administratorLogin: 'pgadmin'
    administratorLoginPassword: dbAdminPassword
    storage: {
      storageSizeGB: 32
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource postgresFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2022-12-01' = {
  parent: postgresServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}



resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2022-12-01' = {
  parent: postgresServer
  name: 'SchoolERPSystem'
}

var pgConnectionString = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Port=5432;Database=SchoolERPSystem;Username=pgadmin;Password=${dbAdminPassword};Ssl Mode=VerifyFull;'

resource secretDb 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'ConnectionStrings--Default'
  properties: {
    value: pgConnectionString
  }
}

// Generate a random Job API Key
var jobApiKey = uniqueString(resourceGroup().id, deployment().name, 'jobApiKey')

resource secretJobApiKey 'Microsoft.KeyVault/vaults/secrets@2023-02-01' = {
  parent: keyVault
  name: 'JobApiKey'
  properties: {
    value: jobApiKey
  }
}

// 7. Create App Service Plan for Azure Functions
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: 'asp-schoolerp-${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

// 8. Create Azure Function App
resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: 'func-schoolerp-${uniqueString(resourceGroup().id)}'
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      use32BitWorkerProcess: false
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: blobConnectionString
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: blobConnectionString
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower('func-schoolerp-${uniqueString(resourceGroup().id)}')
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'API_URL'
          value: 'http://4.247.233.14' // Will be dynamically replaced by script
        }
        {
          name: 'API_KEY'
          value: jobApiKey
        }
      ]
    }
  }
}

output keyVaultUri string = keyVault.properties.vaultUri
output storageAccountName string = storageAccount.name
output containerName string = blobContainer.name
output staticWebAppDefaultHostName string = staticWebApp.properties.defaultHostname
output acrLoginServer string = acr.properties.loginServer
output aksClusterName string = aks.name
output postgresServerName string = postgresServer.name
