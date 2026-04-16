# Keystone Insurance — Target Architecture Specification

> **Version:** 1.0  
> **Generated:** 2026-04-16  
> **Source:** Spec2Cloud reverse-engineering of `KeystoneInsurance.Core` (.NET Framework 4.6.1)  
> **Target:** .NET 9 · Azure Container Apps · Azure SQL Database

---

## 1. Executive Summary

This document defines the target cloud-native architecture for the Keystone Insurance commercial property platform, modernized from a monolithic .NET Framework 4.6.1 / WCF / MSMQ / Crystal Reports application to .NET 9 on Azure.

### Legacy Stack (Current)

| Layer | Technology |
|---|---|
| Runtime | .NET Framework 4.6.1 |
| Web Framework | ASP.NET MVC 5 + jQuery UI |
| ORM | Entity Framework (Database-First EDMX) |
| Integration | WCF (BasicHttpBinding) |
| Messaging | MSMQ (private queues) |
| Reporting | Crystal Reports |
| Authentication | Windows Authentication |
| Database | SQL Server 2012+ (200+ stored procedures) |

### Target Stack (Modernized)

| Layer | Technology |
|---|---|
| Runtime | .NET 9 |
| API Framework | ASP.NET Core Minimal APIs + MediatR (CQRS) |
| ORM | EF Core 9 (Code-First, migrations) |
| Integration | HttpClient + Polly (resilience) |
| Messaging | Azure Service Bus (queues & topics) |
| Reporting/PDF | QuestPDF |
| Authentication | Microsoft Entra ID (OAuth 2.0 / OIDC) |
| Database | Azure SQL Database |
| Hosting | Azure Container Apps |
| Observability | Azure Monitor + Application Insights + OpenTelemetry |

---

## 2. Solution Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Azure Container Apps Environment             │
│                                                                     │
│  ┌──────────────┐  ┌──────────────────┐  ┌─────────────────────┐   │
│  │  API Gateway  │  │  Quoting Service  │  │ Underwriting Service│   │
│  │  (Ingress)   │─▶│  (.NET 9 API)    │  │  (.NET 9 API)      │   │
│  └──────┬───────┘  └────────┬─────────┘  └──────────┬──────────┘   │
│         │                   │                        │              │
│  ┌──────┴───────┐  ┌───────┴──────────┐  ┌──────────┴──────────┐   │
│  │ Policy Svc   │  │ Endorsement Svc  │  │  Renewal Service    │   │
│  │ (.NET 9 API) │  │ (.NET 9 API)     │  │  (.NET 9 API)       │   │
│  └──────────────┘  └──────────────────┘  └─────────────────────┘   │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │              Shared Libraries (NuGet Packages)               │   │
│  │  • KeystoneInsurance.Domain     (entities, value objects)    │   │
│  │  • KeystoneInsurance.Business   (rules engine, calculators)  │   │
│  │  • KeystoneInsurance.Contracts  (API DTOs, events)           │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
         │                    │                       │
    ┌────┴────┐     ┌────────┴─────────┐    ┌────────┴──────────┐
    │ Azure   │     │  Azure Service   │    │   Azure Blob      │
    │ SQL DB  │     │  Bus             │    │   Storage          │
    └─────────┘     │  • policy-issued │    │   (documents/PDF)  │
                    │  • endorsement   │    └───────────────────┘
                    │  • renewal-due   │
                    │  • compliance    │
                    └──────────────────┘
         │
    ┌────┴────────────────────────────┐
    │  External Integrations          │
    │  • Reinsurance API (HttpClient) │
    │  • Regulatory Reporting API     │
    │  • NAIC Statistical Agent       │
    └─────────────────────────────────┘
```

---

## 3. Project Structure

```
KeystoneInsurance/
├── src/
│   ├── KeystoneInsurance.Api/                  # ASP.NET Core Minimal API host
│   │   ├── Endpoints/
│   │   │   ├── QuoteEndpoints.cs
│   │   │   ├── PolicyEndpoints.cs
│   │   │   ├── UnderwritingEndpoints.cs
│   │   │   ├── EndorsementEndpoints.cs
│   │   │   └── RenewalEndpoints.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   └── CorrelationIdMiddleware.cs
│   │   └── Program.cs
│   │
│   ├── KeystoneInsurance.Domain/               # Domain layer (entities + rules)
│   │   ├── Entities/
│   │   │   ├── Client.cs
│   │   │   ├── Quote.cs
│   │   │   ├── Policy.cs
│   │   │   ├── Endorsement.cs
│   │   │   ├── Coverage.cs
│   │   │   ├── Property.cs
│   │   │   ├── RateFactor.cs
│   │   │   └── UnderwritingDecision.cs
│   │   ├── ValueObjects/
│   │   │   ├── Money.cs
│   │   │   ├── Address.cs
│   │   │   └── PolicyNumber.cs
│   │   ├── Enums/
│   │   │   ├── QuoteStatus.cs
│   │   │   ├── PolicyStatus.cs
│   │   │   ├── ConstructionType.cs
│   │   │   ├── OccupancyType.cs
│   │   │   ├── EndorsementType.cs
│   │   │   └── UnderwritingDecisionType.cs
│   │   └── Rules/
│   │       ├── RatingRules.cs
│   │       ├── UnderwritingRules.cs
│   │       └── ComplianceRules.cs
│   │
│   ├── KeystoneInsurance.Application/          # Application layer (CQRS handlers)
│   │   ├── Quotes/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateQuoteCommand.cs
│   │   │   │   └── RecalculateQuoteCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetQuoteByIdQuery.cs
│   │   │       └── SearchQuotesQuery.cs
│   │   ├── Policies/
│   │   ├── Underwriting/
│   │   ├── Endorsements/
│   │   └── Renewals/
│   │
│   ├── KeystoneInsurance.Business/             # Business logic services
│   │   ├── QuotingEngine.cs
│   │   ├── PremiumCalculator.cs
│   │   ├── UnderwritingService.cs
│   │   ├── ComplianceService.cs
│   │   ├── EndorsementService.cs
│   │   ├── RenewalService.cs
│   │   └── PolicyService.cs
│   │
│   ├── KeystoneInsurance.Infrastructure/       # EF Core, messaging, integrations
│   │   ├── Persistence/
│   │   │   ├── KeystoneDbContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Messaging/
│   │   │   ├── ServiceBusPublisher.cs
│   │   │   └── ServiceBusConsumer.cs
│   │   ├── Integration/
│   │   │   ├── ReinsuranceApiClient.cs
│   │   │   └── RegulatoryReportingClient.cs
│   │   ├── Documents/
│   │   │   └── QuestPdfDocumentGenerator.cs
│   │   └── Storage/
│   │       └── BlobStorageService.cs
│   │
│   └── KeystoneInsurance.Contracts/            # Shared DTOs and events
│       ├── Requests/
│       ├── Responses/
│       └── Events/
│
├── tests/
│   ├── KeystoneInsurance.Domain.Tests/
│   ├── KeystoneInsurance.Business.Tests/
│   ├── KeystoneInsurance.Api.Tests/
│   └── KeystoneInsurance.Integration.Tests/
│
├── KeystoneInsurance.sln
└── README.md
```

---

## 4. Azure Infrastructure

### 4.1 Compute — Azure Container Apps

| Setting | Value |
|---|---|
| Environment | `keystone-cae` |
| Container App | `keystone-api` |
| Min replicas | 1 |
| Max replicas | 10 |
| CPU / Memory | 0.5 vCPU / 1 Gi |
| Ingress | External HTTPS (port 8080) |
| Scaling rule | HTTP concurrent requests > 50 |

### 4.2 Database — Azure SQL Database

| Setting | Value |
|---|---|
| SKU | General Purpose (Serverless) |
| vCores | 2–4 (auto-pause at 1 hr idle) |
| Max size | 32 GB |
| Backup retention | 7 days (PITR) |
| Connection | Managed Identity (no passwords) |

### 4.3 Messaging — Azure Service Bus

| Resource | Type | Purpose |
|---|---|---|
| `policy-issued` | Queue | Triggers document generation |
| `endorsement-requested` | Queue | Processes endorsement workflows |
| `renewal-due` | Queue | Queues renewal processing (replaces MSMQ `policyissuance`) |
| `compliance-events` | Topic | Fan-out compliance/regulatory notifications |

### 4.4 Storage — Azure Blob Storage

| Container | Purpose |
|---|---|
| `policy-documents` | Generated policy PDFs (QuestPDF output) |
| `endorsement-documents` | Endorsement PDFs |
| `regulatory-reports` | Quarterly NAIC reports |

### 4.5 Identity & Security

| Concern | Solution |
|---|---|
| User AuthN | Microsoft Entra ID (OIDC) — replaces Windows Auth |
| Service AuthN | Managed Identity for Azure SQL, Service Bus, Blob Storage |
| API AuthZ | JWT Bearer tokens with role claims (`Underwriter`, `Agent`, `Admin`) |
| Secrets | Azure Key Vault (connection strings, external API keys) |

---

## 5. Key Architectural Decisions

### ADR-1: Modular Monolith over Microservices

The legacy codebase is tightly coupled. A modular monolith deployed as a single container provides service boundaries (via projects/namespaces) without the operational overhead of distributed microservices. Services can be extracted later.

### ADR-2: CQRS with MediatR

Separate read and write paths using MediatR handlers. This decouples the API layer from business logic and enables different optimization strategies for reads vs. writes.

### ADR-3: EF Core Code-First Migrations

Replace the legacy Database-First EDMX and 200+ stored procedures with EF Core Code-First models. Business logic moves entirely to C# domain services. Stored procedures are retired.

### ADR-4: Azure Service Bus replaces MSMQ

MSMQ private queues (`.\private$\policyissuance`, `.\private$\endorsements`) are replaced with Azure Service Bus queues, providing cloud-native messaging with dead-letter queues, retry policies, and monitoring.

### ADR-5: QuestPDF replaces Crystal Reports

Crystal Reports is replaced with QuestPDF for programmatic PDF generation of policy documents, endorsements, and regulatory reports. Documents are stored in Azure Blob Storage.

### ADR-6: HttpClient + Polly replaces WCF

WCF BasicHttpBinding clients (reinsurance, regulatory) are replaced with typed `HttpClient` instances configured with Polly resilience policies (retry, circuit breaker, timeout).

---

## 6. Migration Mapping

| Legacy Component | Target Replacement |
|---|---|
| `Web.config` connection strings | Azure Key Vault + Managed Identity |
| `System.ServiceModel` WCF bindings | `HttpClient` + Polly |
| MSMQ `.\private$\policyissuance` | Azure Service Bus `policy-issued` queue |
| MSMQ `.\private$\endorsements` | Azure Service Bus `endorsement-requested` queue |
| Crystal Reports policy templates | QuestPDF `PolicyDocumentGenerator` |
| Windows Authentication | Microsoft Entra ID (OIDC) |
| SQL Server LocalDB | Azure SQL Database |
| `usp_GetQuoteDetails` (stored proc) | EF Core LINQ query in `GetQuoteByIdHandler` |
| `usp_CalculatePremium` (stored proc) | `QuotingEngine.CalculatePremium()` in C# |
| `usp_SearchQuotes` (stored proc) | EF Core dynamic query with `IQueryable` |
| `usp_GetExpiringPolicies` (stored proc) | EF Core query in `RenewalService` |
| `usp_UpdatePolicyStatus` (stored proc) | EF Core update + `AuditLog` interceptor |
| `usp_GetPremiumSummaryByState` (stored proc) | EF Core GroupBy projection |
| Entity Framework EDMX | EF Core `DbContext` with Fluent API configurations |

---

## 7. Observability

| Concern | Implementation |
|---|---|
| Distributed tracing | OpenTelemetry → Application Insights |
| Structured logging | `ILogger<T>` with Serilog sink to App Insights |
| Metrics | Custom metrics for premium calculations, UW decisions |
| Health checks | `/health/ready` and `/health/live` endpoints |
| Alerts | Azure Monitor alerts on error rate, latency P99 |

---

## 8. Deployment

| Stage | Tooling |
|---|---|
| CI | GitHub Actions — build, test, lint |
| Container | Dockerfile (multi-stage: SDK → runtime) |
| CD | GitHub Actions → Azure Container Apps revision |
| IaC | Bicep templates for all Azure resources |
| Environments | `dev` → `staging` → `production` |
