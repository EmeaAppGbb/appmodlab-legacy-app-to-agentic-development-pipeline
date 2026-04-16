// ---------------------------------------------------------------------------
// Keystone Insurance – Azure Infrastructure (Bicep)
// Deploys: Container Apps, Azure SQL, Service Bus, Blob Storage,
//          Application Insights, Redis Cache
// ---------------------------------------------------------------------------

targetScope = 'resourceGroup'

// ── Parameters ──────────────────────────────────────────────────────────────

@description('Base name used to derive all resource names.')
@minLength(3)
@maxLength(20)
param appName string = 'keystone'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Environment tag (dev, staging, production).')
@allowed(['dev', 'staging', 'production'])
param environmentName string = 'dev'

@description('Container image to deploy (e.g. myacr.azurecr.io/keystone-api:latest).')
param containerImage string = ''

@description('Azure SQL administrator login.')
@secure()
param sqlAdminLogin string

@description('Azure SQL administrator password.')
@secure()
param sqlAdminPassword string

@description('Azure Container Registry name (without .azurecr.io).')
param acrName string = ''

@description('Tags applied to every resource.')
param tags object = {
  project: 'keystone-insurance'
  environment: environmentName
  managedBy: 'bicep'
}

// ── Variables ───────────────────────────────────────────────────────────────

var uniqueSuffix = uniqueString(resourceGroup().id, appName)
var resourcePrefix = '${appName}-${environmentName}'
// Storage account names: 3-24 chars, lowercase alphanumeric only
var storageAccountName = toLower('${take(appName, 10)}${take(uniqueSuffix, 10)}st')

// ── Log Analytics Workspace ─────────────────────────────────────────────────

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${resourcePrefix}-law'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// ── Application Insights ────────────────────────────────────────────────────

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${resourcePrefix}-ai'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// ── Azure SQL Server & Database ─────────────────────────────────────────────

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: '${resourcePrefix}-sql'
  location: location
  tags: tags
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'KeystoneInsurance'
  location: location
  tags: tags
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 2
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 34359738368 // 32 GB
    autoPauseDelay: 60
    minCapacity: json('0.5')
    zoneRedundant: false
    requestedBackupStorageRedundancy: 'Local'
  }
}

// Allow Azure services to access SQL
resource sqlFirewallAllowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ── Azure Service Bus ───────────────────────────────────────────────────────

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${resourcePrefix}-sb'
  location: location
  tags: tags
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    disableLocalAuth: false
  }
}

var serviceBusQueues = [
  'policy-issued'
  'endorsement-requested'
  'renewal-due'
]

resource sbQueues 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = [
  for queueName in serviceBusQueues: {
    parent: serviceBusNamespace
    name: queueName
    properties: {
      lockDuration: 'PT1M'
      maxSizeInMegabytes: 1024
      requiresDuplicateDetection: false
      requiresSession: false
      defaultMessageTimeToLive: 'P14D'
      deadLetteringOnMessageExpiration: true
      maxDeliveryCount: 10
      enablePartitioning: false
      enableBatchedOperations: true
    }
  }
]

resource sbTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: 'compliance-events'
  properties: {
    defaultMessageTimeToLive: 'P14D'
    maxSizeInMegabytes: 1024
    enablePartitioning: false
    enableBatchedOperations: true
  }
}

// ── Azure Blob Storage ──────────────────────────────────────────────────────

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: true
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

var blobContainers = [
  'policy-documents'
  'endorsement-documents'
  'regulatory-reports'
]

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = [
  for containerName in blobContainers: {
    parent: blobService
    name: containerName
    properties: {
      publicAccess: 'None'
    }
  }
]

// ── Redis Cache ─────────────────────────────────────────────────────────────

resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: '${resourcePrefix}-redis'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    redisConfiguration: {
      'maxmemory-policy': 'allkeys-lru'
    }
  }
}

// ── Container Apps Environment ──────────────────────────────────────────────

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${resourcePrefix}-cae'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    daprAIInstrumentationKey: appInsights.properties.InstrumentationKey
  }
}

// ── Container App ───────────────────────────────────────────────────────────

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${resourcePrefix}-api'
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      registries: !empty(acrName)
        ? [
            {
              server: '${acrName}.azurecr.io'
              identity: 'system'
            }
          ]
        : []
      secrets: [
        {
          name: 'sql-connection-string'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=KeystoneInsurance;Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'servicebus-connection-string'
          value: listKeys('${serviceBusNamespace.id}/AuthorizationRules/RootManageSharedAccessKey', '2022-10-01-preview').primaryConnectionString
        }
        {
          name: 'storage-connection-string'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
        }
        {
          name: 'redis-connection-string'
          value: '${redisCache.properties.hostName}:6380,password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False'
        }
        {
          name: 'appinsights-connection-string'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'keystone-api'
          image: !empty(containerImage) ? containerImage : 'mcr.microsoft.com/dotnet/samples:aspnetapp'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: environmentName == 'production' ? 'Production' : 'Development'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'ConnectionStrings__KeystoneDb'
              secretRef: 'sql-connection-string'
            }
            {
              name: 'ConnectionStrings__ServiceBus'
              secretRef: 'servicebus-connection-string'
            }
            {
              name: 'ConnectionStrings__BlobStorage'
              secretRef: 'storage-connection-string'
            }
            {
              name: 'ConnectionStrings__Redis'
              secretRef: 'redis-connection-string'
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              secretRef: 'appinsights-connection-string'
            }
          ]
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health/live'
                port: 8080
              }
              initialDelaySeconds: 15
              periodSeconds: 30
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health/ready'
                port: 8080
              }
              initialDelaySeconds: 5
              periodSeconds: 10
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '50'
              }
            }
          }
        ]
      }
    }
  }
}

// ── Outputs ─────────────────────────────────────────────────────────────────

@description('Container App FQDN.')
output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn

@description('Container App URL.')
output containerAppUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'

@description('SQL Server FQDN.')
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName

@description('Service Bus namespace.')
output serviceBusNamespaceName string = serviceBusNamespace.name

@description('Storage account name.')
output storageAccountName string = storageAccount.name

@description('Application Insights connection string.')
output appInsightsConnectionString string = appInsights.properties.ConnectionString

@description('Redis Cache hostname.')
output redisCacheHostName string = redisCache.properties.hostName

@description('Container App managed identity principal ID.')
output containerAppPrincipalId string = containerApp.identity.principalId
