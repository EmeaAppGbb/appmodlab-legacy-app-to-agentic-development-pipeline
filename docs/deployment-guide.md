# Keystone Insurance — Azure Deployment Guide

> **Target Platform:** Azure Container Apps · Azure SQL Database · Azure Service Bus · Azure Blob Storage  
> **IaC:** Bicep templates  
> **CI/CD:** GitHub Actions

---

## Table of Contents

- [Prerequisites](#prerequisites)
- [Architecture Overview](#architecture-overview)
- [Step 1: Resource Group and Environment](#step-1-resource-group-and-environment)
- [Step 2: Azure SQL Database](#step-2-azure-sql-database)
- [Step 3: Azure Service Bus](#step-3-azure-service-bus)
- [Step 4: Azure Blob Storage](#step-4-azure-blob-storage)
- [Step 5: Azure Key Vault](#step-5-azure-key-vault)
- [Step 6: Azure Container Registry](#step-6-azure-container-registry)
- [Step 7: Azure Container Apps](#step-7-azure-container-apps)
- [Step 8: Build and Deploy the Application](#step-8-build-and-deploy-the-application)
- [Step 9: Configure Managed Identity](#step-9-configure-managed-identity)
- [Step 10: Verify Deployment](#step-10-verify-deployment)
- [Bicep Template Reference](#bicep-template-reference)
- [GitHub Actions CI/CD](#github-actions-cicd)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

| Requirement | Details |
|---|---|
| Azure Subscription | With Contributor access |
| Azure CLI | v2.50+ (`az --version`) |
| Docker | v24+ (for container builds) |
| .NET SDK | 9.0+ |
| GitHub Access | For CI/CD pipeline |

```bash
# Login to Azure
az login

# Set subscription
az account set --subscription "<your-subscription-id>"

# Verify CLI tools
az --version
docker --version
dotnet --version
```

---

## Architecture Overview

```
                    ┌─────────────┐
                    │   GitHub    │
                    │   Actions   │
                    └──────┬──────┘
                           │ CI/CD
                    ┌──────▼──────┐
                    │   Azure     │
                    │   Container │
                    │   Registry  │
                    └──────┬──────┘
                           │ Image Pull
┌──────────────────────────▼────────────────────────────────┐
│              Azure Container Apps Environment              │
│                                                            │
│  ┌──────────────────────────────────────────────────────┐  │
│  │           keystone-api (Container App)                │  │
│  │   .NET 9 · Port 8080 · HTTPS Ingress · 1-10 replicas│  │
│  └──────────┬──────────────┬─────────────┬──────────────┘  │
└─────────────│──────────────│─────────────│─────────────────┘
              │              │             │
    ┌─────────▼──┐  ┌───────▼────┐  ┌─────▼──────────┐
    │ Azure SQL  │  │ Service Bus│  │ Blob Storage   │
    │ Database   │  │            │  │                │
    │ Serverless │  │ 3 Queues   │  │ 3 Containers  │
    │ 2-4 vCores │  │ 1 Topic    │  │               │
    └────────────┘  └────────────┘  └────────────────┘
              │
    ┌─────────▼──┐
    │ Key Vault  │
    │ Secrets    │
    └────────────┘
```

---

## Step 1: Resource Group and Environment

### Create Resource Group

```bash
# Variables — customize these
RESOURCE_GROUP="rg-keystone-insurance"
LOCATION="eastus"
ENVIRONMENT="dev"   # dev | staging | production

# Create resource group
az group create \
  --name $RESOURCE_GROUP \
  --location $LOCATION \
  --tags environment=$ENVIRONMENT project=keystone-insurance
```

---

## Step 2: Azure SQL Database

### Create SQL Server and Database

```bash
SQL_SERVER="keystone-sql-${ENVIRONMENT}"
SQL_DB="KeystoneInsurance"

# Create SQL Server (using Entra ID admin, no SQL auth)
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-ad-only-auth \
  --external-admin-principal-type User \
  --external-admin-name "<your-entra-admin-email>" \
  --external-admin-sid "<your-entra-object-id>"

# Create database (Serverless, auto-pause)
az sql db create \
  --name $SQL_DB \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --edition GeneralPurpose \
  --compute-model Serverless \
  --family Gen5 \
  --min-capacity 0.5 \
  --capacity 4 \
  --max-size 32GB \
  --auto-pause-delay 60 \
  --backup-storage-redundancy Local

# Allow Azure services to access
az sql server firewall-rule create \
  --name AllowAzureServices \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### Apply Database Schema

```bash
# Option A: Apply EF Core migrations
cd src/KeystoneInsurance.Modern
dotnet ef database update --connection "Server=${SQL_SERVER}.database.windows.net;Database=${SQL_DB};Authentication=Active Directory Default;"

# Option B: Apply SQL scripts directly
cd database
sqlcmd -S "${SQL_SERVER}.database.windows.net" -d $SQL_DB \
  --authentication-method=ActiveDirectoryDefault \
  -i schema.sql
sqlcmd -S "${SQL_SERVER}.database.windows.net" -d $SQL_DB \
  --authentication-method=ActiveDirectoryDefault \
  -i seed-data.sql
```

---

## Step 3: Azure Service Bus

### Create Namespace, Queues, and Topics

```bash
SB_NAMESPACE="keystone-sb-${ENVIRONMENT}"

# Create namespace (Standard tier for topics support)
az servicebus namespace create \
  --name $SB_NAMESPACE \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard

# Create queues
az servicebus queue create --name policy-issued \
  --namespace-name $SB_NAMESPACE --resource-group $RESOURCE_GROUP \
  --max-delivery-count 5 --default-message-time-to-live P7D \
  --dead-lettering-on-message-expiration true

az servicebus queue create --name endorsement-requested \
  --namespace-name $SB_NAMESPACE --resource-group $RESOURCE_GROUP \
  --max-delivery-count 5 --default-message-time-to-live P7D \
  --dead-lettering-on-message-expiration true

az servicebus queue create --name renewal-due \
  --namespace-name $SB_NAMESPACE --resource-group $RESOURCE_GROUP \
  --max-delivery-count 3 --default-message-time-to-live P14D \
  --dead-lettering-on-message-expiration true

# Create compliance events topic with subscription
az servicebus topic create --name compliance-events \
  --namespace-name $SB_NAMESPACE --resource-group $RESOURCE_GROUP

az servicebus topic subscription create \
  --name regulatory-reporter \
  --topic-name compliance-events \
  --namespace-name $SB_NAMESPACE \
  --resource-group $RESOURCE_GROUP

az servicebus topic subscription create \
  --name audit-logger \
  --topic-name compliance-events \
  --namespace-name $SB_NAMESPACE \
  --resource-group $RESOURCE_GROUP
```

### Queue Configuration Reference

| Queue/Topic | Max Delivery | TTL | Dead-Letter | Purpose |
|---|---|---|---|---|
| `policy-issued` | 5 | 7 days | Yes | Policy document generation |
| `endorsement-requested` | 5 | 7 days | Yes | Endorsement processing |
| `renewal-due` | 3 | 14 days | Yes | Renewal processing |
| `compliance-events` (topic) | — | — | — | Regulatory fan-out notifications |

---

## Step 4: Azure Blob Storage

### Create Storage Account and Containers

```bash
STORAGE_ACCOUNT="keystonestore${ENVIRONMENT}"

# Create storage account
az storage account create \
  --name $STORAGE_ACCOUNT \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku Standard_LRS \
  --kind StorageV2 \
  --allow-blob-public-access false

# Create containers
az storage container create --name policy-documents \
  --account-name $STORAGE_ACCOUNT
az storage container create --name endorsement-documents \
  --account-name $STORAGE_ACCOUNT
az storage container create --name regulatory-reports \
  --account-name $STORAGE_ACCOUNT
```

### Container Purpose

| Container | Contents |
|---|---|
| `policy-documents` | Generated policy PDF documents (QuestPDF output) |
| `endorsement-documents` | Endorsement amendment PDFs |
| `regulatory-reports` | Quarterly NAIC regulatory reports |

---

## Step 5: Azure Key Vault

### Create Key Vault and Store Secrets

```bash
KV_NAME="keystone-kv-${ENVIRONMENT}"

# Create Key Vault
az keyvault create \
  --name $KV_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization true

# Store connection strings (if not using Managed Identity for all)
az keyvault secret set --vault-name $KV_NAME \
  --name "Reinsurance-ApiKey" --value "<reinsurance-api-key>"

az keyvault secret set --vault-name $KV_NAME \
  --name "Regulatory-ApiKey" --value "<regulatory-api-key>"
```

---

## Step 6: Azure Container Registry

### Create ACR and Build Container Image

```bash
ACR_NAME="keystoneacr${ENVIRONMENT}"

# Create Azure Container Registry
az acr create \
  --name $ACR_NAME \
  --resource-group $RESOURCE_GROUP \
  --sku Basic \
  --admin-enabled false

# Build and push container image
az acr build \
  --registry $ACR_NAME \
  --image keystone-api:latest \
  --file src/KeystoneInsurance.Modern/Dockerfile \
  .
```

### Dockerfile (Multi-Stage Build)

Create `src/KeystoneInsurance.Modern/Dockerfile`:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/KeystoneInsurance.Modern/KeystoneInsurance.Modern.csproj ./
RUN dotnet restore

COPY src/KeystoneInsurance.Modern/ ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KeystoneInsurance.Modern.dll"]
```

---

## Step 7: Azure Container Apps

### Create Container Apps Environment and App

```bash
CAE_NAME="keystone-cae-${ENVIRONMENT}"
CA_NAME="keystone-api"

# Create Container Apps environment
az containerapp env create \
  --name $CAE_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Create the Container App
az containerapp create \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --environment $CAE_NAME \
  --image "${ACR_NAME}.azurecr.io/keystone-api:latest" \
  --registry-server "${ACR_NAME}.azurecr.io" \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 10 \
  --cpu 0.5 \
  --memory 1Gi \
  --env-vars \
    "ConnectionStrings__KeystoneDb=Server=${SQL_SERVER}.database.windows.net;Database=${SQL_DB};Authentication=Active Directory Managed Identity;" \
    "ConnectionStrings__ServiceBus=${SB_NAMESPACE}.servicebus.windows.net" \
    "ConnectionStrings__BlobStorage=https://${STORAGE_ACCOUNT}.blob.core.windows.net" \
    "Integration__Reinsurance__BaseUrl=https://reinsurance-partner.example.com" \
    "Integration__Regulatory__BaseUrl=https://naic-reporting.example.com"

# Configure scaling rule
az containerapp update \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --scale-rule-name http-rule \
  --scale-rule-type http \
  --scale-rule-http-concurrency 50
```

---

## Step 8: Build and Deploy the Application

### Manual Deployment

```bash
# Build the container
cd src/KeystoneInsurance.Modern
dotnet publish -c Release

# Push to ACR and update Container App
az acr build --registry $ACR_NAME --image keystone-api:v1.0.0 .

az containerapp update \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --image "${ACR_NAME}.azurecr.io/keystone-api:v1.0.0"
```

### Get Application URL

```bash
# Get the FQDN
az containerapp show \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "properties.configuration.ingress.fqdn" -o tsv
```

---

## Step 9: Configure Managed Identity

### Enable System-Assigned Managed Identity

```bash
# Enable managed identity on Container App
az containerapp identity assign \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --system-assigned

# Get the identity principal ID
IDENTITY_ID=$(az containerapp show \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "identity.principalId" -o tsv)
```

### Grant Role Assignments

```bash
# Azure SQL — allow managed identity login
# (Run this SQL in the database)
# CREATE USER [keystone-api] FROM EXTERNAL PROVIDER;
# ALTER ROLE db_datareader ADD MEMBER [keystone-api];
# ALTER ROLE db_datawriter ADD MEMBER [keystone-api];

# Service Bus — Azure Service Bus Data Owner
SB_RESOURCE_ID=$(az servicebus namespace show \
  --name $SB_NAMESPACE --resource-group $RESOURCE_GROUP --query id -o tsv)

az role assignment create \
  --role "Azure Service Bus Data Owner" \
  --assignee $IDENTITY_ID \
  --scope $SB_RESOURCE_ID

# Blob Storage — Storage Blob Data Contributor
STORAGE_ID=$(az storage account show \
  --name $STORAGE_ACCOUNT --resource-group $RESOURCE_GROUP --query id -o tsv)

az role assignment create \
  --role "Storage Blob Data Contributor" \
  --assignee $IDENTITY_ID \
  --scope $STORAGE_ID

# Key Vault — Key Vault Secrets User
KV_ID=$(az keyvault show --name $KV_NAME --query id -o tsv)

az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $IDENTITY_ID \
  --scope $KV_ID
```

### Role Assignment Summary

| Azure Resource | Role | Purpose |
|---|---|---|
| Azure SQL Database | `db_datareader` + `db_datawriter` | EF Core data access |
| Azure Service Bus | `Azure Service Bus Data Owner` | Send/receive messages |
| Azure Blob Storage | `Storage Blob Data Contributor` | Upload/download documents |
| Azure Key Vault | `Key Vault Secrets User` | Read API keys |

---

## Step 10: Verify Deployment

### Health Check

```bash
APP_URL=$(az containerapp show --name $CA_NAME --resource-group $RESOURCE_GROUP \
  --query "properties.configuration.ingress.fqdn" -o tsv)

# Readiness probe
curl "https://${APP_URL}/health/ready"

# Liveness probe
curl "https://${APP_URL}/health/live"
```

### Functional Test

```bash
# Create a test quote
curl -X POST "https://${APP_URL}/api/v1/quotes" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": 1,
    "propertyAddress": "100 Azure Lane",
    "city": "Seattle",
    "stateCode": "WA",
    "zipCode": "98101",
    "propertyValue": 2000000.00,
    "constructionType": "Fire Resistive",
    "occupancyType": "Office",
    "yearBuilt": 2020,
    "squareFootage": 50000,
    "numberOfStories": 5,
    "sprinklersInstalled": true,
    "alarmSystemInstalled": true,
    "roofType": "Built-Up",
    "roofAge": 3,
    "coverageLimit": 2000000.00,
    "deductible": 50000.00,
    "businessInterruptionCoverage": true,
    "businessInterruptionLimit": 750000.00,
    "equipmentBreakdownCoverage": false,
    "floodCoverage": false,
    "earthquakeCoverage": false,
    "priorClaimsCount": 0,
    "priorClaimsTotalAmount": 0.00
  }'
```

### Monitor Logs

```bash
# Stream Container App logs
az containerapp logs show \
  --name $CA_NAME \
  --resource-group $RESOURCE_GROUP \
  --follow
```

---

## Bicep Template Reference

Below is a complete Bicep template that provisions all required Azure resources. Save as `infra/main.bicep`:

```bicep
targetScope = 'resourceGroup'

@description('Environment name (dev, staging, production)')
param environment string = 'dev'

@description('Azure region')
param location string = resourceGroup().location

@description('Container image to deploy')
param containerImage string = ''

@description('SQL Entra admin object ID')
param sqlAdminObjectId string

@description('SQL Entra admin display name')
param sqlAdminName string

// ---- Naming ----
var suffix = '${environment}-${uniqueString(resourceGroup().id)}'

// ---- SQL Server & Database ----
resource sqlServer 'Microsoft.Sql/servers@2023-05-01-preview' = {
  name: 'keystone-sql-${suffix}'
  location: location
  properties: {
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: sqlAdminName
      sid: sqlAdminObjectId
      tenantId: tenant().tenantId
    }
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2023-05-01-preview' = {
  parent: sqlServer
  name: 'KeystoneInsurance'
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 4
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    maxSizeBytes: 34359738368  // 32 GB
  }
}

resource sqlFirewall 'Microsoft.Sql/servers/firewallRules@2023-05-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// ---- Service Bus ----
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'keystone-sb-${suffix}'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}

resource queuePolicyIssued 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'policy-issued'
  properties: {
    maxDeliveryCount: 5
    defaultMessageTimeToLive: 'P7D'
    deadLetteringOnMessageExpiration: true
  }
}

resource queueEndorsement 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'endorsement-requested'
  properties: {
    maxDeliveryCount: 5
    defaultMessageTimeToLive: 'P7D'
    deadLetteringOnMessageExpiration: true
  }
}

resource queueRenewal 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'renewal-due'
  properties: {
    maxDeliveryCount: 3
    defaultMessageTimeToLive: 'P14D'
    deadLetteringOnMessageExpiration: true
  }
}

resource topicCompliance 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  parent: serviceBus
  name: 'compliance-events'
}

resource subRegulatory 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: topicCompliance
  name: 'regulatory-reporter'
}

resource subAudit 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  parent: topicCompliance
  name: 'audit-logger'
}

// ---- Storage Account ----
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'keystonestore${suffix}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource containerPolicies 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'policy-documents'
}

resource containerEndorsements 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'endorsement-documents'
}

resource containerReports 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'regulatory-reports'
}

// ---- Key Vault ----
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: 'keystone-kv-${suffix}'
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenant().tenantId
    enableRbacAuthorization: true
  }
}

// ---- Container Registry ----
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: 'keystoneacr${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
  }
}

// ---- Container Apps Environment ----
resource cae 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: 'keystone-cae-${environment}'
  location: location
}

// ---- Container App ----
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: 'keystone-api'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: cae.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: acr.properties.loginServer
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'keystone-api'
          image: containerImage != '' ? containerImage : 'mcr.microsoft.com/dotnet/samples:aspnetapp'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ConnectionStrings__KeystoneDb'
              value: 'Server=${sqlServer.properties.fullyQualifiedDomainName};Database=KeystoneInsurance;Authentication=Active Directory Managed Identity;'
            }
            {
              name: 'ConnectionStrings__ServiceBus'
              value: '${serviceBus.name}.servicebus.windows.net'
            }
            {
              name: 'ConnectionStrings__BlobStorage'
              value: storageAccount.properties.primaryEndpoints.blob
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-rule'
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

// ---- Outputs ----
output appUrl string = 'https://${containerApp.properties.configuration.ingress.fqdn}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output serviceBusNamespace string = '${serviceBus.name}.servicebus.windows.net'
output storageEndpoint string = storageAccount.properties.primaryEndpoints.blob
output containerAppIdentityPrincipalId string = containerApp.identity.principalId
```

### Deploy with Bicep

```bash
az deployment group create \
  --resource-group $RESOURCE_GROUP \
  --template-file infra/main.bicep \
  --parameters \
    environment=$ENVIRONMENT \
    sqlAdminObjectId="<your-object-id>" \
    sqlAdminName="<your-email>"
```

---

## GitHub Actions CI/CD

### Workflow File (`.github/workflows/deploy.yml`)

```yaml
name: Build and Deploy

on:
  push:
    branches: [main]
  workflow_dispatch:

env:
  AZURE_RESOURCE_GROUP: rg-keystone-insurance
  CONTAINER_APP_NAME: keystone-api
  ACR_NAME: keystoneacr

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - name: Build and test
        run: |
          dotnet restore
          dotnet build --no-restore
          dotnet test --no-build --verbosity normal

      - name: Azure Login
        uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Build and push to ACR
        run: |
          az acr build \
            --registry ${{ env.ACR_NAME }} \
            --image keystone-api:${{ github.sha }} \
            --file src/KeystoneInsurance.Modern/Dockerfile .

      - name: Deploy to Container Apps
        run: |
          az containerapp update \
            --name ${{ env.CONTAINER_APP_NAME }} \
            --resource-group ${{ env.AZURE_RESOURCE_GROUP }} \
            --image ${{ env.ACR_NAME }}.azurecr.io/keystone-api:${{ github.sha }}
```

---

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|---|---|---|
| `401 Unauthorized` on SQL | Managed Identity not configured | Run `CREATE USER [keystone-api] FROM EXTERNAL PROVIDER` in SQL |
| Service Bus connection refused | Wrong connection string format | Use FQDN only: `namespace.servicebus.windows.net` (no `Endpoint=`) |
| Container won't start | Missing env vars | Check `az containerapp logs show` for startup errors |
| Health check fails | DB not accessible | Verify SQL firewall rules allow Azure services |
| PDF generation OOM | QuestPDF memory usage | Increase container memory to 2Gi |

### Useful Diagnostic Commands

```bash
# View Container App logs
az containerapp logs show --name $CA_NAME --resource-group $RESOURCE_GROUP --follow

# Check Container App status
az containerapp show --name $CA_NAME --resource-group $RESOURCE_GROUP \
  --query "{status:properties.runningStatus, replicas:properties.template.scale}"

# List Container App revisions
az containerapp revision list --name $CA_NAME --resource-group $RESOURCE_GROUP -o table

# Check Service Bus queue metrics
az servicebus queue show --name policy-issued \
  --namespace-name $SB_NAMESPACE --resource-group $RESOURCE_GROUP \
  --query "{messages:countDetails.activeMessageCount, deadLetter:countDetails.deadLetterMessageCount}"
```

---

## Environment Matrix

| Setting | Dev | Staging | Production |
|---|---|---|---|
| SQL vCores | 0.5–2 | 2–4 | 4–8 |
| SQL Auto-pause | 60 min | 120 min | Disabled |
| Container replicas | 1–3 | 1–5 | 2–10 |
| CPU/Memory | 0.25/0.5Gi | 0.5/1Gi | 1.0/2Gi |
| Service Bus SKU | Standard | Standard | Premium |
| Storage redundancy | LRS | GRS | RA-GRS |
| Key Vault | Standard | Standard | Premium |
