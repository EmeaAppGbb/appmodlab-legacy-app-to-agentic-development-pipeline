# Keystone Insurance — Legacy to Modern Migration Guide

> **From:** .NET Framework 4.6.1 · ASP.NET MVC 5 · WCF · MSMQ · Crystal Reports · SQL Server  
> **To:** .NET 9 · ASP.NET Core · Azure Service Bus · QuestPDF · Azure SQL · Azure Container Apps

---

## Table of Contents

- [Migration Overview](#migration-overview)
- [Technology Stack Migration](#technology-stack-migration)
- [1. Project Structure Mapping](#1-project-structure-mapping)
- [2. Data Access Layer](#2-data-access-layer)
- [3. Business Logic Services](#3-business-logic-services)
- [4. Integration Layer](#4-integration-layer)
- [5. Messaging (MSMQ → Service Bus)](#5-messaging-msmq--service-bus)
- [6. Reporting (Crystal Reports → QuestPDF)](#6-reporting-crystal-reports--questpdf)
- [7. Authentication (Windows Auth → Entra ID)](#7-authentication-windows-auth--entra-id)
- [8. Configuration (Web.config → appsettings.json)](#8-configuration-webconfig--appsettingsjson)
- [9. Stored Procedure Migration](#9-stored-procedure-migration)
- [10. UI Layer (jQuery UI → Blazor Server)](#10-ui-layer-jquery-ui--blazor-server)
- [11. Database Migration](#11-database-migration)
- [12. Deployment (IIS → Container Apps)](#12-deployment-iis--container-apps)
- [Workshop Exercise Reference](#workshop-exercise-reference)

---

## Migration Overview

The Keystone Insurance modernization transforms a monolithic .NET Framework application into a cloud-native .NET 9 application. This guide maps every legacy component to its modern replacement to help workshop participants understand the migration decisions.

```
┌─ LEGACY ──────────────────────────────┐     ┌─ MODERN ─────────────────────────────┐
│                                        │     │                                       │
│  .NET Framework 4.6.1                  │ ──▶ │  .NET 9                               │
│  ASP.NET MVC 5 + jQuery UI            │ ──▶ │  ASP.NET Core Controllers + Blazor    │
│  Entity Framework (Database-First EDMX)│ ──▶ │  EF Core 9 (Code-First + Fluent API) │
│  WCF BasicHttpBinding                  │ ──▶ │  HttpClient + Polly Resilience        │
│  MSMQ Private Queues                   │ ──▶ │  Azure Service Bus                    │
│  Crystal Reports                       │ ──▶ │  QuestPDF                             │
│  Windows Authentication                │ ──▶ │  Microsoft Entra ID (OIDC + JWT)      │
│  SQL Server (200+ stored procs)        │ ──▶ │  Azure SQL + EF Core LINQ             │
│  IIS on Windows Server                 │ ──▶ │  Azure Container Apps (Linux)          │
│  Web.config                            │ ──▶ │  appsettings.json + Key Vault         │
│                                        │     │                                       │
└────────────────────────────────────────┘     └───────────────────────────────────────┘
```

---

## Technology Stack Migration

| Layer | Legacy | Modern | Key Change |
|---|---|---|---|
| **Runtime** | .NET Framework 4.6.1 | .NET 9 | Cross-platform, high-performance |
| **Web Framework** | ASP.NET MVC 5 | ASP.NET Core Controllers | Minimal API-style with `[ApiController]` |
| **UI** | jQuery UI + Razor Views | Blazor Server (Interactive) | Component-based, C# everywhere |
| **ORM** | Entity Framework (EDMX) | EF Core 9 (Code-First) | Fluent API, migrations, interceptors |
| **Integration** | WCF (BasicHttpBinding) | HttpClient + Polly | Retry, circuit breaker, timeout |
| **Messaging** | MSMQ (private queues) | Azure Service Bus | Cloud-native, dead-letter, monitoring |
| **Reports** | Crystal Reports | QuestPDF | Programmatic C# PDF generation |
| **Auth** | Windows Authentication | Entra ID (OIDC) | Cloud identity, JWT, RBAC |
| **Database** | SQL Server 2012+ (local) | Azure SQL Database | Serverless, managed, auto-scale |
| **Hosting** | IIS on Windows Server | Azure Container Apps | Containerized, auto-scaling |
| **Config** | Web.config (XML) | appsettings.json + Env Vars | Hierarchical, environment-based |
| **Secrets** | Web.config connectionStrings | Azure Key Vault + Managed Identity | No secrets in code or config files |
| **Observability** | Windows Event Log | Application Insights + OpenTelemetry | Distributed tracing, structured logging |
| **CI/CD** | Manual / MSBuild | GitHub Actions | Automated build → test → deploy |

---

## 1. Project Structure Mapping

### Legacy Structure

```
KeystoneInsurance.sln
├── KeystoneInsurance.Core/        # All business logic + data access
│   ├── Domain/                    #   Entities (Quote, Policy, Client...)
│   ├── Services/                  #   QuotingEngine, UnderwritingService...
│   ├── Integration/               #   WCF clients (Reinsurance, Regulatory)
│   └── Properties/                #   Assembly info
│
├── KeystoneInsurance.Web/         # ASP.NET MVC 5 web application
│   ├── Controllers/               #   MVC controllers
│   ├── Views/                     #   Razor views + jQuery UI
│   ├── Web.config                 #   All configuration (WCF, MSMQ, auth)
│   └── App_Start/                 #   Route config, bundle config
│
└── database/
    ├── schema.sql                 #   Table definitions
    ├── seed-data.sql              #   Sample data
    └── stored-procedures.sql      #   200+ stored procedures
```

### Modern Structure

```
src/KeystoneInsurance.Modern/
├── Controllers/                   # REST API endpoints (replaces MVC Controllers)
├── Domain/                        # Domain model (same entities, modernized)
│   ├── Entities/                  #   Code-First entity classes
│   ├── Enums/                     #   Strongly-typed enumerations
│   ├── Rules/                     #   Business rules (replaces stored procs)
│   └── ValueObjects/              #   Immutable value types
├── Services/                      # Business services (from Core/Services)
├── Data/                          # EF Core DbContext (replaces EDMX)
│   ├── KeystoneDbContext.cs
│   ├── AuditSaveChangesInterceptor.cs
│   └── Configurations/            #   Fluent API configurations
├── Integration/                   # External integrations
│   ├── Reinsurance/               #   HttpClient (replaces WCF)
│   ├── Regulatory/                #   HttpClient (replaces WCF)
│   └── ServiceBus/                #   Azure SB (replaces MSMQ)
├── Documents/                     # QuestPDF (replaces Crystal Reports)
├── Components/                    # Blazor Server UI (replaces jQuery views)
└── Program.cs                     # DI + startup (replaces Web.config + Global.asax)
```

### Component-by-Component Mapping

| Legacy File/Folder | Modern Equivalent | Notes |
|---|---|---|
| `KeystoneInsurance.Core/Domain/` | `Domain/Entities/` | Entity properties preserved; added navigation props |
| `KeystoneInsurance.Core/Services/QuotingEngine.cs` | `Services/QuotingEngine.cs` | Same business logic, async/await pattern |
| `KeystoneInsurance.Core/Services/UnderwritingService.cs` | `Services/UnderwritingService.cs` | Uses `UnderwritingRules` static helper |
| `KeystoneInsurance.Core/Services/PremiumCalculator.cs` | `Services/PremiumCalculator.cs` | Same 40+ rating factors |
| `KeystoneInsurance.Core/Integration/ReinsuranceClient.cs` | `Integration/Reinsurance/ReinsuranceApiClient.cs` | WCF → HttpClient + Polly |
| `KeystoneInsurance.Core/Integration/RegulatoryReporter.cs` | `Integration/Regulatory/RegulatoryReportingClient.cs` | WCF → HttpClient + Polly |
| `KeystoneInsurance.Web/Controllers/` | `Controllers/` | MVC → API controllers |
| `KeystoneInsurance.Web/Views/` | `Components/Pages/` | Razor views → Blazor components |
| `KeystoneInsurance.Web/Web.config` | `appsettings.json` + `Program.cs` | XML → JSON + code-based DI |
| `database/stored-procedures.sql` | `Services/` + `Domain/Rules/` | SQL → C# LINQ + domain rules |

---

## 2. Data Access Layer

### EDMX → EF Core Code-First

**Legacy (Database-First EDMX):**
- 100+ entities auto-generated from SQL schema
- `.edmx` file with designer surface
- `ObjectContext` for data access
- No migration support — schema changes via SQL scripts

**Modern (EF Core Code-First):**
- Hand-crafted entity classes with `[Required]`, `[MaxLength]` etc.
- Fluent API configurations in `Data/Configurations/`
- `DbContext` with `DbSet<T>` properties
- Migration-based schema management

### DbContext Registration

```csharp
// Legacy: in Web.config
// <connectionStrings>
//   <add name="KeystoneEntities" connectionString="..." providerName="System.Data.EntityClient" />
// </connectionStrings>

// Modern: in Program.cs
builder.Services.AddDbContext<KeystoneDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(new AuditSaveChangesInterceptor());
});
```

### Entity Comparison

```csharp
// Legacy: Auto-generated from EDMX
public partial class Quote
{
    public int QuoteID { get; set; }          // PascalCase ID suffix
    public Nullable<int> ClientID { get; set; } // Nullable<T> syntax
    public string QuoteNumber { get; set; }
    // ... 30+ properties with no documentation
}

// Modern: Hand-crafted with clear organization
public class Quote
{
    public int QuoteId { get; set; }           // Consistent Id suffix
    public int ClientId { get; set; }          // Non-nullable FK

    // Property Information (grouped)
    public string PropertyAddress { get; set; } = null!;
    public string StateCode { get; set; } = null!;
    // ...

    // Navigation properties
    public Client Client { get; set; } = null!;
    public UnderwritingDecision? UnderwritingDecision { get; set; }
    public Policy? Policy { get; set; }
}
```

### Audit Logging

```csharp
// Legacy: Manual audit logging in each service method
// connection.Execute("INSERT INTO AuditLog ...");

// Modern: Automatic via EF Core SaveChanges interceptor
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    // Automatically captures all entity changes
    // Writes to AuditLog table on every SaveChanges call
}
```

---

## 3. Business Logic Services

All business logic has been preserved with the same algorithms and factors. The key change is moving from synchronous to async patterns.

### Service Interface Pattern

```csharp
// Legacy: Synchronous, no interface
public class QuotingEngine
{
    public Quote CreateQuote(Quote quote) { ... }
}

// Modern: Async with interface for DI/testing
public interface IQuotingEngine
{
    Task<Quote> CreateQuoteAsync(Quote quote, CancellationToken ct = default);
    Task<Quote> RecalculateAsync(int quoteId, CancellationToken ct = default);
    List<string> ValidateQuote(Quote quote);
}
```

### DI Registration

```csharp
// Legacy: new QuotingEngine() instantiated directly
// Modern: Registered in Program.cs
builder.Services.AddScoped<IPremiumCalculator, PremiumCalculator>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IQuotingEngine, QuotingEngine>();
builder.Services.AddScoped<IUnderwritingService, UnderwritingService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
```

### Premium Calculator — Same Factors, New Patterns

The 40+ rating factors are preserved identically:

| Factor Category | # Rules | Legacy Location | Modern Location |
|---|---|---|---|
| Base Rate (20 states) | 20 | `QuotingEngine.GetBaseRate()` | `PremiumCalculator.GetBaseRate()` |
| Property Value | 7 | `QuotingEngine.GetPropertyValueFactor()` | `PremiumCalculator.GetPropertyValueFactor()` |
| Construction Type | 6 | `QuotingEngine.GetConstructionFactor()` | `PremiumCalculator.GetConstructionFactor()` |
| Building Age | 8 | `QuotingEngine.GetAgeFactor()` | `PremiumCalculator.GetAgeFactor()` |
| Occupancy Class | 11 | `QuotingEngine.GetOccupancyFactor()` | `PremiumCalculator.GetOccupancyFactor()` |
| Protection | 4 | `QuotingEngine.GetProtectionCredit()` | `PremiumCalculator.GetProtectionCredit()` |
| Territory/Cat | varies | `QuotingEngine.GetCatastropheFactor()` | `PremiumCalculator.GetCatastropheFactor()` |
| Deductible Credit | varies | `QuotingEngine.GetDeductibleCredit()` | `PremiumCalculator.GetDeductibleCredit()` |

---

## 4. Integration Layer

### WCF → HttpClient + Polly

**Legacy WCF Configuration (`Web.config`):**

```xml
<system.serviceModel>
  <client>
    <endpoint name="ReinsuranceService"
              address="http://reinsurance-partner.example.com/service"
              binding="basicHttpBinding"
              contract="IReinsuranceService" />
  </client>
</system.serviceModel>
```

**Modern HttpClient + Polly (`Program.cs`):**

```csharp
builder.Services.AddHttpClient<IReinsuranceApiClient, ReinsuranceApiClient>(client =>
{
    client.BaseAddress = new Uri(config["Integration:Reinsurance:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("reinsurance", pipeline =>
{
    // Retry: 3 attempts with exponential backoff
    pipeline.AddRetry(new HttpRetryStrategyOptions { ... });
    // Circuit breaker: 50% failure → 30s break
    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions { ... });
    // Timeout: 10s per attempt
    pipeline.AddTimeout(TimeSpan.FromSeconds(10));
});
```

### Migration Mapping

| Legacy (WCF) | Modern (HttpClient) |
|---|---|
| `BasicHttpBinding` | `HttpClient` with `System.Text.Json` |
| `<endpoint>` in Web.config | `AddHttpClient<T>()` in DI |
| Sync `.CedeRisk()` | `async Task<T> CedeRiskAsync()` |
| No retry logic | Polly retry (3 attempts, exponential backoff) |
| No circuit breaker | Polly circuit breaker (50% failure → 30s break) |
| SOAP XML serialization | JSON serialization |
| Transport security | HTTPS + API key from Key Vault |

---

## 5. Messaging (MSMQ → Service Bus)

### Queue Mapping

| Legacy MSMQ Queue | Modern Service Bus Resource | Type | Purpose |
|---|---|---|---|
| `.\private$\policyissuance` | `policy-issued` | Queue | Policy document generation |
| `.\private$\endorsements` | `endorsement-requested` | Queue | Endorsement processing |
| *(not present)* | `renewal-due` | Queue | Renewal processing (new feature) |
| *(not present)* | `compliance-events` | Topic | Regulatory notifications (new) |

### Code Comparison

```csharp
// Legacy: MSMQ direct queue access
using System.Messaging;

var queue = new MessageQueue(@".\private$\policyissuance");
queue.Send(new PolicyIssuanceMessage { PolicyId = 10 });

// Modern: Azure Service Bus publisher
public class PolicyIssuancePublisher
{
    private readonly ServiceBusClient _client;

    public async Task PublishAsync(PolicyIssuedMessage message, CancellationToken ct)
    {
        var sender = _client.CreateSender("policy-issued");
        await sender.SendMessageAsync(
            new ServiceBusMessage(JsonSerializer.Serialize(message)), ct);
    }
}
```

### Background Workers

| Worker | Legacy Equivalent | Modern Implementation |
|---|---|---|
| `PolicyDocumentWorker` | Windows Service polling MSMQ | `BackgroundService` consuming Service Bus |
| `EndorsementProcessorWorker` | Windows Service polling MSMQ | `BackgroundService` consuming Service Bus |
| `RenewalProcessorWorker` | SQL Agent job | `BackgroundService` consuming Service Bus |

---

## 6. Reporting (Crystal Reports → QuestPDF)

### Migration Approach

| Legacy | Modern |
|---|---|
| Crystal Reports `.rpt` template files | C# code in `QuestPdfDocumentGenerator.cs` |
| Runtime requires Crystal Reports Runtime installed | NuGet package `QuestPDF` — zero runtime dependency |
| Template-based (visual designer) | Programmatic Fluent API |
| Output to printer or file | Output to Azure Blob Storage |

### Code Comparison

```csharp
// Legacy: Crystal Reports
var report = new ReportDocument();
report.Load("PolicyDocument.rpt");
report.SetParameterValue("PolicyNumber", policy.PolicyNumber);
report.ExportToDisk(ExportFormatType.PortableDocFormat, outputPath);

// Modern: QuestPDF
public class QuestPdfDocumentGenerator : IPolicyDocumentGenerator
{
    public byte[] GeneratePolicyDocument(Policy policy)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Content().Column(col =>
                {
                    col.Item().Text($"Policy: {policy.PolicyNumber}");
                    col.Item().Text($"Premium: {policy.AnnualPremium:C}");
                    // ... detailed layout
                });
            });
        }).GeneratePdf();
    }
}
```

---

## 7. Authentication (Windows Auth → Entra ID)

| Aspect | Legacy | Modern |
|---|---|---|
| Protocol | NTLM / Kerberos | OAuth 2.0 / OIDC |
| Identity Provider | Active Directory (on-prem) | Microsoft Entra ID (cloud) |
| Token Type | Windows ticket | JWT Bearer token |
| Role Claims | AD Groups | Entra App Roles (`Agent`, `Underwriter`, `Admin`) |
| Service Auth | N/A (trusted network) | Managed Identity |
| Configuration | `<authentication mode="Windows"/>` | `AddAuthentication().AddJwtBearer()` |

### Roles Mapping

| Legacy AD Group | Modern Entra Role | Capabilities |
|---|---|---|
| `KEYSTONE\Agents` | `Agent` | Create/view quotes, view policies |
| `KEYSTONE\Underwriters` | `Underwriter` | + Evaluate UW, approve endorsements |
| `KEYSTONE\SeniorUW` | `SeniorUnderwriter` | + Referral decisions, high-value |
| `KEYSTONE\Admins` | `Admin` | Full access |

---

## 8. Configuration (Web.config → appsettings.json)

### Legacy Web.config (XML)

```xml
<configuration>
  <connectionStrings>
    <add name="KeystoneEntities"
         connectionString="Data Source=.;Initial Catalog=KeystoneInsurance;Integrated Security=True"
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  <appSettings>
    <add key="MSMQPolicyQueue" value=".\private$\policyissuance" />
    <add key="MSMQEndorsementQueue" value=".\private$\endorsements" />
  </appSettings>
  <system.serviceModel>
    <client>
      <endpoint name="ReinsuranceService"
                address="http://reinsurance-partner.example.com/service"
                binding="basicHttpBinding" />
    </client>
  </system.serviceModel>
  <system.web>
    <authentication mode="Windows" />
  </system.web>
</configuration>
```

### Modern appsettings.json (JSON)

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
      "RetryAttempts": 3
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
    }
  }
}
```

### Configuration Mapping

| Legacy Config | Modern Config |
|---|---|
| `connectionStrings/KeystoneEntities` | `ConnectionStrings:KeystoneDb` |
| `appSettings/MSMQPolicyQueue` | `ServiceBus:Queues:PolicyIssued` |
| `appSettings/MSMQEndorsementQueue` | `ServiceBus:Queues:EndorsementRequested` |
| `system.serviceModel/endpoint[@name='ReinsuranceService']` | `Integration:Reinsurance:BaseUrl` |
| `system.web/authentication[@mode='Windows']` | Entra ID config in `Program.cs` |

---

## 9. Stored Procedure Migration

All 200+ stored procedures have been replaced with EF Core LINQ queries and C# business logic.

### Key Stored Procedure Mappings

| Legacy Stored Procedure | Modern Replacement | Location |
|---|---|---|
| `usp_GetQuoteDetails` | EF Core `.Include()` LINQ query | `QuotesController.GetQuote()` |
| `usp_CalculatePremium` | `QuotingEngine.CreateQuoteAsync()` | `Services/QuotingEngine.cs` |
| `usp_SearchQuotes` | EF Core dynamic `IQueryable` | `QuotesController.SearchQuotes()` |
| `usp_GetExpiringPolicies` | EF Core date filter query | `RenewalProcessorWorker` |
| `usp_UpdatePolicyStatus` | EF Core update + `AuditLog` interceptor | `PolicyService.CancelPolicyAsync()` |
| `usp_GetPremiumSummaryByState` | EF Core `GroupBy` projection | Reports endpoint |
| `usp_InsertAuditLog` | `AuditSaveChangesInterceptor` | `Data/AuditSaveChangesInterceptor.cs` |

### Example: Search Quotes

```sql
-- Legacy: usp_SearchQuotes
CREATE PROCEDURE usp_SearchQuotes
    @ClientID INT = NULL,
    @StateCode VARCHAR(2) = NULL,
    @Status VARCHAR(20) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SELECT q.*, c.BusinessName
    FROM Quotes q
    JOIN Clients c ON q.ClientID = c.ClientID
    WHERE (@ClientID IS NULL OR q.ClientID = @ClientID)
      AND (@StateCode IS NULL OR q.StateCode = @StateCode)
      AND (@Status IS NULL OR q.Status = @Status)
      AND (@FromDate IS NULL OR q.CreatedDate >= @FromDate)
      AND (@ToDate IS NULL OR q.CreatedDate <= @ToDate)
    ORDER BY q.CreatedDate DESC
END
```

```csharp
// Modern: EF Core IQueryable with dynamic filters
var query = _db.Quotes.Include(q => q.Client).AsQueryable();

if (clientId.HasValue) query = query.Where(q => q.ClientId == clientId.Value);
if (!string.IsNullOrEmpty(stateCode)) query = query.Where(q => q.StateCode == stateCode);
if (!string.IsNullOrEmpty(status)) query = query.Where(q => q.Status == status);
if (fromDate.HasValue) query = query.Where(q => q.CreatedDate >= fromDate.Value);
if (toDate.HasValue) query = query.Where(q => q.CreatedDate <= toDate.Value);

var totalCount = await query.CountAsync(ct);
var items = await query
    .OrderByDescending(q => q.CreatedDate)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(q => new { q.QuoteId, q.QuoteNumber, ... })
    .ToListAsync(ct);
```

---

## 10. UI Layer (jQuery UI → Blazor Server)

### Page Mapping

| Legacy MVC View | Modern Blazor Component | Route |
|---|---|---|
| `Views/Quote/Index.cshtml` | `Components/Pages/QuoteList.razor` | `/quotes` |
| `Views/Quote/Create.cshtml` | `Components/Pages/QuoteCreate.razor` | `/quotes/create` |
| `Views/Quote/Details.cshtml` | `Components/Pages/QuoteDetail.razor` | `/quotes/{id}` |
| `Views/Policy/Index.cshtml` | `Components/Pages/PolicyList.razor` | `/policies` |
| `Views/Policy/Details.cshtml` | `Components/Pages/PolicyDetail.razor` | `/policies/{id}` |
| `Views/Underwriting/Review.cshtml` | `Components/Pages/UnderwritingReview.razor` | `/underwriting` |

### Key Differences

| Aspect | Legacy (MVC + jQuery) | Modern (Blazor Server) |
|---|---|---|
| Rendering | Server-side HTML + client-side jQuery | Server-side C# components with SignalR |
| Data Binding | jQuery AJAX calls to MVC actions | C# `@bind` + API client service |
| Forms | HTML forms + jQuery validation | Blazor `EditForm` + `DataAnnotations` |
| Interactivity | jQuery UI widgets | Native Blazor interactive components |
| State | Client-side jQuery state | Server-side component state |

---

## 11. Database Migration

### Schema Changes

| Aspect | Legacy | Modern |
|---|---|---|
| Primary Keys | `INT IDENTITY` | `int` with EF Core value generation |
| String Types | `VARCHAR`/`NVARCHAR` | C# `string` with `MaxLength` config |
| Money Columns | `DECIMAL(15,2)` | `decimal` with `Precision(15, 2)` |
| Booleans | `BIT` | `bool` |
| Audit Columns | Manual `DEFAULT GETDATE()` | EF Core `ValueGeneratedOnAdd` |
| Relationships | Foreign keys only | Navigation properties + FK properties |

### New Tables

| Table | Purpose | Legacy Equivalent |
|---|---|---|
| `AuditLogs` | Automatic change tracking | Manual audit stored proc |
| `Coverages` | Coverage option reference data | Embedded in stored procs |
| `RateFactors` | Rating factor reference data | Hardcoded in stored procs |

---

## 12. Deployment (IIS → Container Apps)

| Aspect | Legacy | Modern |
|---|---|---|
| Host | IIS on Windows Server | Azure Container Apps (Linux) |
| Process Model | w3wp.exe (IIS worker) | Kestrel in Docker container |
| Scaling | Manual (add servers) | Auto-scale (HTTP concurrency rule) |
| Configuration | Web.config transforms | Environment variables + Key Vault |
| SSL | IIS certificate binding | Container Apps managed TLS |
| Health Checks | None | `/health/ready` + `/health/live` |
| Deployment | XCOPY / Web Deploy | Container image → ACR → Container Apps |
| Monitoring | Windows Event Log | Application Insights + OpenTelemetry |

---

## Workshop Exercise Reference

Use this migration guide as a reference while working through the workshop exercises. Each section maps to a specific modernization pattern that SQUAD agents implement automatically when working from Spec2Cloud-generated specifications.

### Suggested Exploration Order

1. **Start with the API** — Call `POST /api/v1/quotes` and trace through `QuotesController` → `QuotingEngine` → `PremiumCalculator`
2. **Evaluate Underwriting** — Call `POST /api/v1/underwriting/evaluate` and review the risk scoring in `UnderwritingService`
3. **Issue a Policy** — Call `POST /api/v1/policies` and see how the quote status check works
4. **Create Endorsements** — Call the three endorsement endpoints and observe premium change calculations
5. **Explore Data Layer** — Review `KeystoneDbContext` and `AuditSaveChangesInterceptor`
6. **Review Integrations** — Study the HttpClient + Polly configuration in `Program.cs`
7. **Check Background Workers** — Understand how Service Bus handlers process async work

### Key Files to Compare

| What to Compare | Legacy File | Modern File |
|---|---|---|
| Premium Calculation | `KeystoneInsurance.Core/Services/QuotingEngine.cs` | `Services/QuotingEngine.cs` |
| Underwriting Rules | `KeystoneInsurance.Core/Services/UnderwritingService.cs` | `Services/UnderwritingService.cs` + `Domain/Rules/UnderwritingRules.cs` |
| Data Access | `KeystoneInsurance.Core/Domain/` (EDMX-generated) | `Domain/Entities/` + `Data/KeystoneDbContext.cs` |
| WCF Integration | `KeystoneInsurance.Core/Integration/ReinsuranceClient.cs` | `Integration/Reinsurance/ReinsuranceApiClient.cs` |
| Configuration | `KeystoneInsurance.Web/Web.config` | `appsettings.json` + `Program.cs` |
| Stored Procedures | `database/stored-procedures.sql` | `Controllers/` + `Services/` (inline LINQ) |
