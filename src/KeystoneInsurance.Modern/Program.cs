using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Services;
using Microsoft.EntityFrameworkCore;

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
