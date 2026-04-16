using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Documents;
using KeystoneInsurance.Modern.Integration.Reinsurance;
using KeystoneInsurance.Modern.Integration.Regulatory;
using KeystoneInsurance.Modern.Integration.ServiceBus.Handlers;
using KeystoneInsurance.Modern.Integration.ServiceBus.Publishers;
using KeystoneInsurance.Modern.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<KeystoneDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("KeystoneDb")
        ?? "Server=(localdb)\\mssqllocaldb;Database=KeystoneInsurance;Trusted_Connection=True;";
    options.UseSqlServer(connectionString);
    options.AddInterceptors(new AuditSaveChangesInterceptor());
});

// Business services
builder.Services.AddScoped<IPremiumCalculator, PremiumCalculator>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IQuotingEngine, QuotingEngine>();
builder.Services.AddScoped<IUnderwritingService, UnderwritingService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

// --- Azure Service Bus ---
builder.Services.AddSingleton(_ =>
    new ServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus")));

builder.Services.AddSingleton<IMessagePublisher, ServiceBusPublisher>();
builder.Services.AddSingleton<PolicyIssuancePublisher>();
builder.Services.AddSingleton<EndorsementPublisher>();
builder.Services.AddHostedService<PolicyDocumentWorker>();
builder.Services.AddHostedService<EndorsementProcessorWorker>();
builder.Services.AddHostedService<RenewalProcessorWorker>();

// --- Azure Blob Storage ---
builder.Services.AddSingleton(_ =>
    new BlobServiceClient(builder.Configuration.GetConnectionString("BlobStorage")));

// --- QuestPDF Document Generation ---
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
builder.Services.AddScoped<IPolicyDocumentGenerator, QuestPdfDocumentGenerator>();

// --- Reinsurance API Client (HttpClient + Polly) ---
builder.Services.AddHttpClient<IReinsuranceApiClient, ReinsuranceApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Integration:Reinsurance:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddResilienceHandler("reinsurance", pipeline =>
{
    pipeline.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromSeconds(2),
        BackoffType = DelayBackoffType.Exponential,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => r.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
    });

    pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        SamplingDuration = TimeSpan.FromSeconds(30),
        FailureRatio = 0.5,
        MinimumThroughput = 5,
        BreakDuration = TimeSpan.FromSeconds(30)
    });

    pipeline.AddTimeout(TimeSpan.FromSeconds(10));
});

// --- Regulatory Reporting Client (HttpClient + Polly) ---
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

// API
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddProblemDetails();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<KeystoneDbContext>();

var app = builder.Build();

// Middleware
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoints
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

app.Run();
