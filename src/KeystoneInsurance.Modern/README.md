# Keystone Insurance — Modernized Application

> **Runtime:** .NET 9 · ASP.NET Core · EF Core 9  
> **Cloud:** Azure Container Apps · Azure SQL · Azure Service Bus · Azure Blob Storage  
> **UI:** Blazor Server (Interactive)

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Reference](#api-reference)
- [Business Services](#business-services)
- [Background Workers](#background-workers)
- [Blazor UI](#blazor-ui)
- [Health Checks](#health-checks)
- [Testing](#testing)

---

## Overview

Keystone Insurance Modern is a **cloud-native commercial property insurance platform** modernized from a legacy .NET Framework 4.6.1 monolith. It handles the full insurance lifecycle:

- **Quote Generation** — 40+ rating factors, 50-state compliance, premium calculation
- **Underwriting** — Automated risk scoring with approve/decline/refer decisions
- **Policy Issuance** — Bind approved quotes, generate PDF documents
- **Endorsements** — Coverage changes, deductible adjustments, cancellations
- **Renewals** — Automated expiration detection with inflation-adjusted re-quoting
- **Reporting** — Premium summaries by state and year

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                        Azure Container Apps Environment                      │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────────┐  │
│  │               KeystoneInsurance.Modern (.NET 9 App)                    │  │
│  │                                                                         │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  ┌────────────┐  │  │
│  │  │  Controllers  │  │   Services   │  │   Domain    │  │    Data    │  │  │
│  │  │              │  │              │  │             │  │            │  │  │
│  │  │ • Quotes     │  │ • QuotingEng │  │ • Entities  │  │ • DbCtx   │  │  │
│  │  │ • Policies   │  │ • Premium    │  │ • Rules     │  │ • Configs  │  │  │
│  │  │ • Underwrite │  │ • UW Service │  │ • ValueObj  │  │ • Audit   │  │  │
│  │  │ • Endorse    │  │ • Compliance │  │ • Enums     │  │           │  │  │
│  │  │              │  │ • Policy Svc │  │             │  │           │  │  │
│  │  └──────┬───────┘  └──────┬───────┘  └─────────────┘  └─────┬──────┘  │  │
│  │         │                 │                                   │         │  │
│  │  ┌──────┴─────────────────┴───────────────────────────────────┴──────┐  │  │
│  │  │                    Integration Layer                              │  │  │
│  │  │  • Service Bus Publishers/Handlers (policy-issued, endorsement)  │  │  │
│  │  │  • Reinsurance API Client (HttpClient + Polly)                  │  │  │
│  │  │  • Regulatory Reporting Client (HttpClient + Polly)             │  │  │
│  │  │  • QuestPDF Document Generator                                  │  │  │
│  │  └──────────────────────────────────────────────────────────────────┘  │  │
│  │                                                                         │  │
│  │  ┌──────────────────────────────────────────────────────────────────┐   │  │
│  │  │              Blazor Server UI (Interactive)                      │   │  │
│  │  │  • QuoteList / QuoteCreate / QuoteDetail                       │   │  │
│  │  │  • PolicyList / PolicyDetail                                    │   │  │
│  │  │  • UnderwritingReview                                          │   │  │
│  │  └──────────────────────────────────────────────────────────────────┘   │  │
│  └─────────────────────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────────────────┘
         │                        │                        │
    ┌────┴─────┐          ┌───────┴────────┐       ┌───────┴──────────┐
    │ Azure    │          │ Azure Service  │       │ Azure Blob       │
    │ SQL DB   │          │ Bus            │       │ Storage          │
    │          │          │ • policy-issued│       │ • policy-documents│
    │ EF Core  │          │ • endorsement  │       │ • endorsement-docs│
    │ Code-1st │          │ • renewal-due  │       │ • regulatory-rpts │
    └──────────┘          │ • compliance   │       └──────────────────┘
                          └────────────────┘
```

### Key Architectural Decisions

| Decision | Rationale |
|---|---|
| **Modular Monolith** | Clear project boundaries without microservice overhead; extract later if needed |
| **EF Core Code-First** | Replaces 200+ stored procedures; business logic lives in C# |
| **Azure Service Bus** | Replaces MSMQ; provides dead-letter, retry, monitoring |
| **QuestPDF** | Replaces Crystal Reports; programmatic PDF generation |
| **HttpClient + Polly** | Replaces WCF; adds retry, circuit breaker, timeout resilience |
| **Blazor Server** | Modern interactive UI replacing jQuery UI / ASP.NET MVC 5 |

---

## Project Structure

```
src/KeystoneInsurance.Modern/
├── Controllers/                    # REST API endpoints
│   ├── QuotesController.cs         #   POST/GET /api/v1/quotes
│   ├── PoliciesController.cs       #   POST/GET /api/v1/policies
│   ├── UnderwritingController.cs   #   POST /api/v1/underwriting/evaluate
│   └── EndorsementsController.cs   #   POST/GET /api/v1/endorsements
│
├── Domain/                         # Domain model layer
│   ├── Entities/                   #   Client, Quote, Policy, Endorsement,
│   │   │                           #   UnderwritingDecision, Coverage,
│   │   │                           #   Property, RateFactor, AuditLog
│   │   └── ...
│   ├── Enums/                      #   QuoteStatus, PolicyStatus, etc.
│   ├── Rules/                      #   RatingRules, UnderwritingRules,
│   │                               #   ComplianceRules
│   └── ValueObjects/               #   Money, Address, PolicyNumber
│
├── Services/                       # Business logic services
│   ├── QuotingEngine.cs            #   Quote creation, premium calculation
│   ├── PremiumCalculator.cs        #   40+ rating factor calculations
│   ├── UnderwritingService.cs      #   Risk scoring, decision engine
│   ├── ComplianceService.cs        #   50-state regulatory compliance
│   └── PolicyService.cs            #   Policy issuance, cancellation
│
├── Data/                           # Persistence layer
│   ├── KeystoneDbContext.cs         #   EF Core DbContext
│   ├── AuditSaveChangesInterceptor.cs  # Automatic audit logging
│   └── Configurations/             #   Fluent API entity configurations
│
├── Integration/                    # External system integrations
│   ├── Reinsurance/                #   Reinsurance API client
│   ├── Regulatory/                 #   NAIC regulatory reporting client
│   └── ServiceBus/                 #   Azure Service Bus
│       ├── Publishers/             #     PolicyIssuancePublisher,
│       │                           #     EndorsementPublisher
│       ├── Handlers/               #     PolicyDocumentWorker,
│       │                           #     EndorsementProcessorWorker,
│       │                           #     RenewalProcessorWorker
│       └── Messages/               #     Message DTOs
│
├── Documents/                      # PDF generation
│   ├── IPolicyDocumentGenerator.cs #   Generator interface
│   └── QuestPdfDocumentGenerator.cs#   QuestPDF implementation
│
├── Components/                     # Blazor Server UI
│   ├── App.razor                   #   Root component
│   ├── Routes.razor                #   Routing configuration
│   ├── Layout/                     #   Shared layout components
│   ├── Pages/                      #   Page components
│   │   ├── QuoteList.razor         #     List/search quotes
│   │   ├── QuoteCreate.razor       #     Create new quote form
│   │   ├── QuoteDetail.razor       #     Quote detail view
│   │   ├── PolicyList.razor        #     List policies
│   │   ├── PolicyDetail.razor      #     Policy detail view
│   │   └── UnderwritingReview.razor#     UW decision interface
│   └── Services/                   #   Blazor API client services
│
├── Program.cs                      # Application startup / DI configuration
├── appsettings.json                # Production configuration
├── appsettings.Development.json    # Development overrides
└── KeystoneInsurance.Modern.csproj # Project file (.NET 9)
```

---

## Getting Started

### Prerequisites

| Tool | Version | Purpose |
|---|---|---|
| .NET SDK | 9.0+ | Build and run the application |
| SQL Server | 2019+ or LocalDB | Database (Azure SQL in production) |
| Visual Studio 2022 | 17.8+ | IDE (or VS Code with C# Dev Kit) |
| Azure CLI | 2.50+ | Deploy to Azure (optional) |

### 1. Clone the Repository

```bash
git clone https://github.com/EmeaAppGbb/appmodlab-legacy-app-to-agentic-development-pipeline.git
cd appmodlab-legacy-app-to-agentic-development-pipeline
```

### 2. Set Up the Database

```bash
# Create database and seed data
cd database
sqlcmd -S "(localdb)\mssqllocaldb" -i schema.sql
sqlcmd -S "(localdb)\mssqllocaldb" -i seed-data.sql
sqlcmd -S "(localdb)\mssqllocaldb" -i stored-procedures.sql
```

Or use EF Core migrations (if migrations have been generated):

```bash
cd src/KeystoneInsurance.Modern
dotnet ef database update
```

### 3. Build and Run

```bash
cd src/KeystoneInsurance.Modern
dotnet restore
dotnet build
dotnet run
```

The application starts on:
- **HTTPS:** `https://localhost:5001`
- **HTTP:** `http://localhost:5236`

### 4. Verify Setup

```bash
# Health check
curl https://localhost:5001/health/ready

# Create a test quote
curl -X POST https://localhost:5001/api/v1/quotes \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": 1,
    "propertyAddress": "123 Industrial Pkwy",
    "city": "Chicago",
    "stateCode": "IL",
    "zipCode": "60601",
    "propertyValue": 1500000.00,
    "constructionType": "Non-Combustible",
    "occupancyType": "Manufacturing-Light",
    "yearBuilt": 2010,
    "squareFootage": 25000,
    "numberOfStories": 2,
    "sprinklersInstalled": true,
    "alarmSystemInstalled": true,
    "roofType": "TPO/EPDM",
    "roofAge": 5,
    "coverageLimit": 1500000.00,
    "deductible": 25000.00,
    "businessInterruptionCoverage": true,
    "businessInterruptionLimit": 500000.00,
    "equipmentBreakdownCoverage": false,
    "floodCoverage": false,
    "earthquakeCoverage": false,
    "priorClaimsCount": 0,
    "priorClaimsTotalAmount": 0.00
  }'
```

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "KeystoneDb": "Server=(localdb)\\mssqllocaldb;Database=KeystoneInsurance;Trusted_Connection=True;",
    "ServiceBus": "keystone-sb.servicebus.windows.net",
    "BlobStorage": "https://keystonestorage.blob.core.windows.net"
  },
  "Integration": {
    "Reinsurance": {
      "BaseUrl": "https://reinsurance-partner.example.com",
      "TimeoutSeconds": 30,
      "RetryAttempts": 3,
      "CircuitBreakerThreshold": 5
    },
    "Regulatory": {
      "BaseUrl": "https://naic-reporting.example.com",
      "TimeoutSeconds": 60,
      "RetryAttempts": 2
    }
  },
  "ServiceBus": {
    "Queues": {
      "PolicyIssued": "policy-issued",
      "EndorsementRequested": "endorsement-requested",
      "RenewalDue": "renewal-due"
    },
    "Topics": {
      "ComplianceEvents": "compliance-events"
    }
  },
  "BlobStorage": {
    "Containers": {
      "PolicyDocuments": "policy-documents",
      "EndorsementDocuments": "endorsement-documents",
      "RegulatoryReports": "regulatory-reports"
    }
  }
}
```

### Environment Variables (Production)

| Variable | Description |
|---|---|
| `ConnectionStrings__KeystoneDb` | Azure SQL connection string (or use Managed Identity) |
| `ConnectionStrings__ServiceBus` | Service Bus namespace FQDN |
| `ConnectionStrings__BlobStorage` | Blob Storage endpoint URL |
| `Integration__Reinsurance__BaseUrl` | Reinsurance partner API URL |
| `Integration__Regulatory__BaseUrl` | NAIC regulatory API URL |

---

## API Reference

All endpoints are prefixed with `/api/v1`. See [docs/api-reference.md](../../docs/api-reference.md) for the complete reference.

### Quick Reference

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/quotes` | Create a new quote with premium calculation |
| `GET` | `/api/v1/quotes/{quoteId}` | Retrieve a quote by ID |
| `GET` | `/api/v1/quotes` | Search/filter quotes with pagination |
| `PUT` | `/api/v1/quotes/{quoteId}/recalculate` | Recalculate premium |
| `POST` | `/api/v1/policies` | Issue a policy from an approved quote |
| `GET` | `/api/v1/policies/{policyId}` | Retrieve a policy by ID |
| `POST` | `/api/v1/policies/{policyId}/cancel` | Cancel an active policy |
| `POST` | `/api/v1/underwriting/evaluate` | Evaluate a quote for underwriting |
| `POST` | `/api/v1/endorsements/coverage-change` | Create coverage change endorsement |
| `POST` | `/api/v1/endorsements/deductible-change` | Create deductible change endorsement |
| `POST` | `/api/v1/endorsements/cancellation` | Create cancellation endorsement |
| `GET` | `/api/v1/endorsements` | List endorsements by policy/status |

---

## Business Services

### QuotingEngine

Orchestrates quote creation with 40+ premium rating factors:

- **Base Rate** — State-specific (20 states mapped, default $700)
- **Property Value Factor** — 7 tiers from $100K to $5M+
- **Construction Factor** — 6 types (Frame=1.45 → Fire Resistive=0.65)
- **Building Age Factor** — 8 bands (<5yr=0.90 → 75+yr=1.60)
- **Occupancy Factor** — 11 classifications with state adjustments
- **Protection Credits** — Sprinklers (-15%), alarms (-5-10%)
- **Territory/Catastrophe** — FL hurricane, CA earthquake, TX hail
- **Deductible Credits** — Higher deductible = lower premium
- **State Compliance** — Minimum premiums, mandatory coverages

### UnderwritingService

Automated risk evaluation engine:

| Risk Score | Claims ≥ 3 | High Cat | Decision |
|---|---|---|---|
| > 85 | — | — | **Declined** |
| — | — | PML > 50% | **Declined** |
| > 70 | — | — | **ReferToSenior** |
| — | Yes | — | **ReferToSenior** |
| > 60 | — | — | **RequestMoreInfo** |
| ≤ 60 | No | No | **Approved** |

### PremiumCalculator

Core calculation engine implementing:
- Base rate lookup by state and construction type
- Multi-factor premium adjustments
- Deductible credit calculations
- Installment amount calculations (Annual, Semi-Annual, Quarterly, Monthly)
- Return premium calculations (ProRata, ShortRate, Flat)

### ComplianceService

50-state regulatory compliance checks including:
- **California:** Proposition 103, earthquake disclosure
- **Florida:** Wind mitigation credits, roof age restrictions
- **Texas:** Minimum 1% wind/hail deductible
- **New York:** Coverage-to-value requirements
- **Louisiana:** Coastal parish regulations

---

## Background Workers

Three `BackgroundService` workers process asynchronous tasks via Azure Service Bus:

| Worker | Queue | Purpose |
|---|---|---|
| `PolicyDocumentWorker` | `policy-issued` | Generates policy PDFs via QuestPDF, stores in Blob Storage |
| `EndorsementProcessorWorker` | `endorsement-requested` | Processes endorsements, generates amendment documents |
| `RenewalProcessorWorker` | `renewal-due` | Creates renewal quotes with inflation/trend adjustments |

---

## Blazor UI

The application includes a Blazor Server interactive UI:

| Page | Route | Description |
|---|---|---|
| Quote List | `/quotes` | Search and browse quotes |
| Quote Create | `/quotes/create` | New quote entry form |
| Quote Detail | `/quotes/{id}` | View quote details and premium breakdown |
| Policy List | `/policies` | Browse active policies |
| Policy Detail | `/policies/{id}` | View policy coverage and endorsements |
| UW Review | `/underwriting` | Underwriting review and decision interface |

---

## Health Checks

| Endpoint | Description |
|---|---|
| `GET /health/ready` | Readiness probe — checks database connectivity |
| `GET /health/live` | Liveness probe — confirms application is running |

---

## Testing

### Run Tests

```bash
# From the solution root
dotnet test KeystoneInsurance.sln

# Run specific test project
dotnet test tests/KeystoneInsurance.Domain.Tests/
dotnet test tests/KeystoneInsurance.Business.Tests/
dotnet test tests/KeystoneInsurance.Api.Tests/
dotnet test tests/KeystoneInsurance.Integration.Tests/
```

### Sample Test Scenarios

1. **California Earthquake Risk** — State: CA, Construction: Frame, $1.5M property → High premium
2. **Florida Hurricane Risk** — State: FL, Masonry, Sprinklers → Hurricane surcharge + sprinkler credit
3. **Texas Hail Zone** — State: TX, Roof age 18yr → Roof age surcharge + hail factor

---

## NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.* | SQL Server ORM |
| `Microsoft.EntityFrameworkCore.Design` | 9.* | EF Core migrations tooling |
| `Azure.Messaging.ServiceBus` | 7.* | Azure Service Bus client |
| `Azure.Storage.Blobs` | 12.* | Azure Blob Storage client |
| `Azure.Identity` | 1.* | Managed Identity authentication |
| `Microsoft.Extensions.Http.Resilience` | 9.* | Polly-based HTTP resilience |
| `QuestPDF` | 2024.* | PDF document generation |

---

## License

This project is part of the App Modernization GBB training series. For internal Microsoft use.
