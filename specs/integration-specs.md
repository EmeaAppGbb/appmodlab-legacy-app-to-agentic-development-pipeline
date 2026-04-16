# Keystone Insurance — Integration Specifications

> **Version:** 1.0  
> **Generated:** 2026-04-16  
> **Scope:** WCF → HttpClient+Polly, MSMQ → Azure Service Bus, Crystal Reports → QuestPDF

---

## 1. WCF → HttpClient + Polly

### 1.1 Legacy State

The legacy application uses WCF `BasicHttpBinding` (configured in `Web.config` `system.serviceModel`) for:

1. **Reinsurance Service** — `ReinsuranceClient.cs`
   - Endpoint: `http://reinsurance-partner.example.com/service`
   - Binding: `BasicHttpBinding` with Transport security
   - Contract: `IReinsuranceService`
   - Operations: `CedeRisk()`, `SubmitClaim()`

2. **Regulatory Reporting** — `RegulatoryReporter.cs`
   - Endpoint: `https://naic-reporting.example.com/api`
   - Operations: `SubmitQuarterlyReport()`, `SubmitRateFilingToState()`, `SubmitPolicyTransaction()`

### 1.2 Target Architecture

Replace all WCF clients with typed `HttpClient` instances registered via `IHttpClientFactory`, configured with Polly resilience pipelines.

### 1.3 Reinsurance API Client

```csharp
public interface IReinsuranceApiClient
{
    Task<ReinsuranceCessionResponse> CedeRiskAsync(
        ReinsuranceCessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ReinsuranceRecoveryResponse> SubmitClaimAsync(
        ReinsuranceClaimRequest request,
        CancellationToken cancellationToken = default);
}

public class ReinsuranceApiClient : IReinsuranceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReinsuranceApiClient> _logger;

    public ReinsuranceApiClient(HttpClient httpClient, ILogger<ReinsuranceApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ReinsuranceCessionResponse> CedeRiskAsync(
        ReinsuranceCessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/cessions", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content
            .ReadFromJsonAsync<ReinsuranceCessionResponse>(cancellationToken))!;
    }

    public async Task<ReinsuranceRecoveryResponse> SubmitClaimAsync(
        ReinsuranceClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/claims", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return (await response.Content
            .ReadFromJsonAsync<ReinsuranceRecoveryResponse>(cancellationToken))!;
    }
}
```

### 1.4 Regulatory Reporting API Client

```csharp
public interface IRegulatoryReportingClient
{
    Task<RegulatorySubmissionResponse> SubmitQuarterlyReportAsync(
        QuarterlyReportData data, CancellationToken ct = default);

    Task<RegulatorySubmissionResponse> SubmitRateFilingAsync(
        string stateCode, RateFilingData data, CancellationToken ct = default);

    Task SubmitPolicyTransactionAsync(
        string stateCode, PolicyTransactionData data, CancellationToken ct = default);
}
```

### 1.5 Polly Resilience Configuration

```csharp
// Program.cs — DI registration
builder.Services.AddHttpClient<IReinsuranceApiClient, ReinsuranceApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Integration:Reinsurance:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("reinsurance", pipeline =>
{
    // Retry: 3 attempts with exponential backoff
    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
    });

    // Circuit breaker: open after 5 failures in 30s, stay open 30s
    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(30),
        FailureRatio = 0.5,
        MinimumThroughput = 5,
        BreakDuration = TimeSpan.FromSeconds(30)
    });

    // Timeout per attempt
    pipeline.AddTimeout(TimeSpan.FromSeconds(10));
});

builder.Services.AddHttpClient<IRegulatoryReportingClient, RegulatoryReportingClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Integration:Regulatory:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(60);
})
.AddResilienceHandler("regulatory", pipeline =>
{
    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 2,
        Delay = TimeSpan.FromSeconds(5),
        BackoffType = DelayBackoffType.Exponential
    });
    pipeline.AddTimeout(TimeSpan.FromSeconds(30));
});
```

### 1.6 Configuration

```json
{
  "Integration": {
    "Reinsurance": {
      "BaseUrl": "https://reinsurance-partner.example.com",
      "ApiKey": "{{from-keyvault}}"
    },
    "Regulatory": {
      "BaseUrl": "https://naic-reporting.example.com",
      "ApiKey": "{{from-keyvault}}"
    }
  }
}
```

### 1.7 Migration Mapping

| Legacy (WCF) | Modern (HttpClient) |
|---|---|
| `BasicHttpBinding` | `HttpClient` with `System.Text.Json` |
| `<endpoint>` in Web.config | `AddHttpClient<T>()` in DI |
| Synchronous `.CedeRisk()` | `async Task<T> CedeRiskAsync()` |
| No retry logic | Polly retry (3 attempts, exponential backoff) |
| No circuit breaker | Polly circuit breaker (50% failure → 30s break) |
| SOAP XML serialization | JSON serialization (`PostAsJsonAsync`) |
| Transport security (SSL) | HTTPS + API key header from Key Vault |

---

## 2. MSMQ → Azure Service Bus

### 2.1 Legacy State

MSMQ private queues configured in `Web.config`:

| Queue Path | Purpose |
|---|---|
| `.\private$\policyissuance` | Queues policy document generation after binding |
| `.\private$\endorsements` | Queues endorsement processing and document generation |

### 2.2 Target Architecture

Azure Service Bus queues and topics replace MSMQ, providing cloud-native messaging with dead-letter queues, scheduled delivery, sessions, and monitoring.

### 2.3 Queue/Topic Design

| Resource | Type | Purpose | Subscribers |
|---|---|---|---|
| `policy-issued` | Queue | Policy document generation | Document Generator |
| `endorsement-requested` | Queue | Endorsement processing | Endorsement Processor |
| `renewal-due` | Queue | Renewal processing (60-day advance) | Renewal Processor |
| `compliance-events` | Topic | Regulatory notifications | Regulatory Reporter, Audit Logger |

### 2.4 Message Contracts

#### PolicyIssuedMessage

```csharp
public record PolicyIssuedMessage
{
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = null!;
    public int QuoteId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public decimal AnnualPremium { get; init; }
    public string PaymentPlan { get; init; } = null!;
    public string StateCode { get; init; } = null!;
    public DateTime IssuedAt { get; init; }
    public string IssuedBy { get; init; } = null!;
}
```

#### EndorsementRequestedMessage

```csharp
public record EndorsementRequestedMessage
{
    public int EndorsementId { get; init; }
    public string EndorsementNumber { get; init; } = null!;
    public int PolicyId { get; init; }
    public string EndorsementType { get; init; } = null!;
    public DateTime EffectiveDate { get; init; }
    public decimal? PremiumChange { get; init; }
    public DateTime RequestedAt { get; init; }
}
```

#### RenewalDueMessage

```csharp
public record RenewalDueMessage
{
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = null!;
    public DateTime ExpirationDate { get; init; }
    public decimal CurrentPremium { get; init; }
    public int ClientId { get; init; }
    public string StateCode { get; init; } = null!;
}
```

### 2.5 Publisher Service

```csharp
public interface IMessagePublisher
{
    Task PublishPolicyIssuedAsync(PolicyIssuedMessage message, CancellationToken ct = default);
    Task PublishEndorsementRequestedAsync(EndorsementRequestedMessage message, CancellationToken ct = default);
    Task PublishRenewalDueAsync(RenewalDueMessage message, CancellationToken ct = default);
    Task PublishComplianceEventAsync(ComplianceEventMessage message, CancellationToken ct = default);
}

public class ServiceBusPublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(ServiceBusClient client, ILogger<ServiceBusPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task PublishPolicyIssuedAsync(
        PolicyIssuedMessage message, CancellationToken ct = default)
    {
        var sender = _client.CreateSender("policy-issued");
        var sbMessage = new ServiceBusMessage(
            BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = "PolicyIssued",
            MessageId = $"policy-{message.PolicyId}-{message.IssuedAt:yyyyMMddHHmmss}",
            ApplicationProperties =
            {
                ["StateCode"] = message.StateCode,
                ["PolicyNumber"] = message.PolicyNumber
            }
        };

        await sender.SendMessageAsync(sbMessage, ct);
        _logger.LogInformation("Published PolicyIssued for {PolicyNumber}", message.PolicyNumber);
    }

    // Similar implementations for other message types...
}
```

### 2.6 Consumer Service (Background Worker)

```csharp
public class PolicyDocumentWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyDocumentWorker> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor("policy-issued", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
        });

        processor.ProcessMessageAsync += async args =>
        {
            var message = args.Message.Body.ToObjectFromJson<PolicyIssuedMessage>();

            using var scope = _scopeFactory.CreateScope();
            var documentGenerator = scope.ServiceProvider
                .GetRequiredService<IPolicyDocumentGenerator>();

            await documentGenerator.GeneratePolicyDocumentAsync(message.PolicyId);
            await args.CompleteMessageAsync(args.Message, stoppingToken);
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception,
                "Error processing policy-issued message: {Source}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);

        // Keep alive until cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
```

### 2.7 DI Registration

```csharp
// Program.cs
builder.Services.AddSingleton(_ =>
    new ServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus")));

builder.Services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();
builder.Services.AddHostedService<PolicyDocumentWorker>();
builder.Services.AddHostedService<EndorsementProcessorWorker>();
builder.Services.AddHostedService<RenewalProcessorWorker>();
```

### 2.8 Migration Mapping

| Legacy (MSMQ) | Modern (Azure Service Bus) |
|---|---|
| `.\private$\policyissuance` | `policy-issued` queue |
| `.\private$\endorsements` | `endorsement-requested` queue |
| No dead-letter handling | Built-in DLQ per queue |
| Binary formatter serialization | JSON (`System.Text.Json`) |
| `MessageQueue.Send()` | `ServiceBusSender.SendMessageAsync()` |
| `MessageQueue.Receive()` | `ServiceBusProcessor` with auto-lock renewal |
| No retry/backoff | Service Bus retry policy + DLQ after max delivery |
| Local-only | Cloud-native with geo-redundancy option |
| No monitoring | Azure Monitor metrics + Application Insights traces |

### 2.9 Dead Letter & Error Handling

| Setting | Value |
|---|---|
| Max delivery attempts | 10 |
| Lock duration | 5 minutes |
| Auto-lock renewal | 10 minutes |
| DLQ forwarding | Automatic after max delivery |
| DLQ monitoring | Azure Monitor alert on DLQ depth > 0 |

---

## 3. Crystal Reports → QuestPDF

### 3.1 Legacy State

Crystal Reports is used for generating:
1. Policy declaration documents
2. Endorsement documents
3. Quarterly regulatory/NAIC reports
4. Premium calculation breakdown reports

Documents are stored at file system paths:
- `Policy.PolicyDocumentPath`
- `Endorsement.EndorsementDocumentPath`

### 3.2 Target Architecture

QuestPDF replaces Crystal Reports for programmatic PDF generation. Generated documents are stored in Azure Blob Storage instead of local file paths.

### 3.3 Document Generator Interface

```csharp
public interface IPolicyDocumentGenerator
{
    Task<DocumentResult> GeneratePolicyDocumentAsync(int policyId);
    Task<DocumentResult> GenerateEndorsementDocumentAsync(int endorsementId);
    Task<DocumentResult> GenerateQuoteSummaryAsync(int quoteId);
    Task<DocumentResult> GenerateRenewalOfferAsync(int renewalQuoteId);
}

public record DocumentResult
{
    public string BlobUrl { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public long FileSizeBytes { get; init; }
    public DateTime GeneratedAt { get; init; }
}
```

### 3.4 QuestPDF Policy Document Implementation

```csharp
public class QuestPdfDocumentGenerator : IPolicyDocumentGenerator
{
    private readonly KeystoneDbContext _dbContext;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<QuestPdfDocumentGenerator> _logger;

    public async Task<DocumentResult> GeneratePolicyDocumentAsync(int policyId)
    {
        var policy = await _dbContext.Policies
            .Include(p => p.Quote).ThenInclude(q => q.Client)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.PolicyId == policyId)
            ?? throw new InvalidOperationException($"Policy {policyId} not found");

        var pdfBytes = GeneratePolicyPdf(policy);
        var fileName = $"policies/{policy.PolicyNumber}/declaration-{DateTime.UtcNow:yyyyMMdd}.pdf";

        var containerClient = _blobServiceClient.GetBlobContainerClient("policy-documents");
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(new BinaryData(pdfBytes), overwrite: true);

        // Update policy record
        policy.PolicyDocumentPath = blobClient.Uri.ToString();
        policy.DocumentGeneratedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new DocumentResult
        {
            BlobUrl = blobClient.Uri.ToString(),
            FileName = fileName,
            FileSizeBytes = pdfBytes.Length,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private byte[] GeneratePolicyPdf(Policy policy)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Text("KEYSTONE INSURANCE")
                            .FontSize(20).Bold();
                        row.ConstantItem(150).Text("Commercial Property\nDeclaration Page")
                            .AlignRight();
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        // Policy Information Section
                        col.Item().Text($"Policy Number: {policy.PolicyNumber}").Bold();
                        col.Item().Text($"Effective: {policy.EffectiveDate:d} to {policy.ExpirationDate:d}");
                        col.Item().Text($"Issue Date: {policy.IssueDate:d}");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        // Insured Information
                        col.Item().Text("NAMED INSURED").Bold();
                        col.Item().Text(policy.Quote.Client.BusinessName);
                        col.Item().Text(policy.Quote.PropertyAddress);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        // Coverage Summary
                        col.Item().Text("COVERAGE SUMMARY").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });
                            table.Cell().Text("Coverage Limit");
                            table.Cell().Text($"${policy.CoverageLimit:N0}");
                            table.Cell().Text("Deductible");
                            table.Cell().Text($"${policy.Deductible:N0}");
                            table.Cell().Text("Annual Premium");
                            table.Cell().Text($"${policy.AnnualPremium:N2}");
                            table.Cell().Text("Payment Plan");
                            table.Cell().Text(policy.PaymentPlan);
                        });
                    });
                });

                page.Footer().AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
```

### 3.5 Blob Storage Configuration

```csharp
// Program.cs
builder.Services.AddSingleton(_ =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("BlobStorage")));

builder.Services.AddScoped<IPolicyDocumentGenerator, QuestPdfDocumentGenerator>();
```

### 3.6 Storage Structure

```
policy-documents/
├── KIP20260501-A1B2C3D4E5/
│   ├── declaration-20260416.pdf
│   └── declaration-20260501.pdf
endorsement-documents/
├── KIP20260501-A1B2C3D4E5-END20260416201000/
│   └── endorsement-20260416.pdf
regulatory-reports/
├── 2026/
│   ├── Q1-quarterly-financial.pdf
│   └── Q2-quarterly-financial.pdf
```

### 3.7 Migration Mapping

| Legacy (Crystal Reports) | Modern (QuestPDF + Blob Storage) |
|---|---|
| Crystal Reports `.rpt` templates | QuestPDF fluent C# document builder |
| Crystal Reports runtime (SAP dependency) | QuestPDF NuGet package (MIT license) |
| Local file system paths | Azure Blob Storage URLs |
| `Policy.PolicyDocumentPath` (file path) | `Policy.PolicyDocumentPath` (blob URL) |
| Synchronous generation | Async generation triggered by Service Bus |
| No versioning | Blob Storage with date-stamped file names |
| Windows-only (COM interop) | Cross-platform (.NET 9) |
| Manual template editing | Code-first document definition (version-controlled) |

### 3.8 Document Types

| Document | Trigger | Storage Container |
|---|---|---|
| Policy Declaration | `policy-issued` queue message | `policy-documents` |
| Endorsement Document | `endorsement-requested` queue message | `endorsement-documents` |
| Quote Summary | On-demand via API | `policy-documents` |
| Renewal Offer Letter | Renewal workflow | `policy-documents` |
| Quarterly NAIC Report | Scheduled (quarterly) | `regulatory-reports` |
| Premium Breakdown | On-demand via API | `policy-documents` |

---

## 4. Configuration Summary

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=keystone-sql.database.windows.net;Database=KeystoneInsurance;Authentication=Active Directory Managed Identity",
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

---

## 5. NuGet Packages Required

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.Extensions.Http.Polly` | 9.x | HttpClient resilience |
| `Microsoft.Extensions.Http.Resilience` | 9.x | Polly v8 integration |
| `Azure.Messaging.ServiceBus` | 7.x | Azure Service Bus client |
| `Azure.Storage.Blobs` | 12.x | Azure Blob Storage client |
| `QuestPDF` | 2024.x | PDF document generation |
| `Azure.Identity` | 1.x | Managed Identity authentication |
| `Azure.Security.KeyVault.Secrets` | 4.x | Key Vault access |
