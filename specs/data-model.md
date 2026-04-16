# Keystone Insurance — EF Core Code-First Data Model

> **Version:** 1.0  
> **Generated:** 2026-04-16  
> **ORM:** Entity Framework Core 9  
> **Database:** Azure SQL Database  
> **Source:** Migrated from legacy SQL Server schema (`database/schema.sql`) + EDMX entities

---

## 1. Migration Summary

| Legacy | Modernized |
|---|---|
| Database-First EDMX (100+ entities) | EF Core Code-First with Fluent API |
| `INT IDENTITY` primary keys | Retained `int` PKs (compatibility) |
| `VARCHAR`/`NVARCHAR` strings | String properties with `MaxLength` constraints |
| `BIT` columns | `bool` properties |
| `DECIMAL(15,2)` money columns | `decimal` with `Precision(15, 2)` |
| `DATETIME` audit columns | `DateTime` with `ValueGeneratedOnAdd` |
| SQL `DEFAULT` constraints | EF Core `HasDefaultValue` / `HasDefaultValueSql` |
| Stored procedures (6 migrated) | EF Core LINQ queries in application layer |
| Manual audit logging | EF Core `SaveChangesInterceptor` for `AuditLog` |

---

## 2. Entity Models

### 2.1 Client

```csharp
public class Client
{
    public int ClientId { get; set; }
    public string ClientNumber { get; set; } = null!;
    public string BusinessName { get; set; } = null!;
    public string? ContactFirstName { get; set; }
    public string? ContactLastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Business Information
    public string? BusinessType { get; set; }
    public int YearsInBusiness { get; set; }
    public string? FederalTaxId { get; set; }

    // Address
    public string? MailingAddress { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingZip { get; set; }

    // Account Information
    public DateTime AccountCreatedDate { get; set; }
    public string AccountStatus { get; set; } = "Active";
    public decimal? CreditScore { get; set; }

    // Risk Profile
    public string? RiskTier { get; set; } // Preferred, Standard, SubStandard
    public int TotalActivePolicies { get; set; }
    public decimal TotalPremiumInForce { get; set; }
    public int ClaimsHistory { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
```

### 2.2 Quote

```csharp
public class Quote
{
    public int QuoteId { get; set; }
    public int ClientId { get; set; }
    public string QuoteNumber { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Status { get; set; } = "Draft";
    // Valid: Draft, Pending, Approved, Declined, Expired, Bound

    // Property Information
    public string PropertyAddress { get; set; } = null!;
    public string City { get; set; } = null!;
    public string StateCode { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string? CountyName { get; set; }
    public decimal PropertyValue { get; set; }
    public string ConstructionType { get; set; } = null!;
    // Valid: Frame, Joisted Masonry, Non-Combustible,
    //        Masonry Non-Combustible, Modified Fire Resistive, Fire Resistive
    public string OccupancyType { get; set; } = null!;
    public int YearBuilt { get; set; }
    public int SquareFootage { get; set; }
    public int NumberOfStories { get; set; }
    public bool SprinklersInstalled { get; set; }
    public bool AlarmSystemInstalled { get; set; }
    public string? RoofType { get; set; }
    public int RoofAge { get; set; }

    // Coverage Details
    public decimal CoverageLimit { get; set; }
    public decimal Deductible { get; set; }
    public bool BusinessInterruptionCoverage { get; set; }
    public decimal? BusinessInterruptionLimit { get; set; }
    public bool EquipmentBreakdownCoverage { get; set; }
    public bool FloodCoverage { get; set; }
    public bool EarthquakeCoverage { get; set; }

    // Loss History
    public int PriorClaimsCount { get; set; }
    public decimal PriorClaimsTotalAmount { get; set; }

    // Premium Calculation Results
    public decimal? BasePremium { get; set; }
    public decimal? TotalPremium { get; set; }
    public string? PremiumCalculationDetails { get; set; }

    // Underwriting
    public int? UnderwriterId { get; set; }
    public string? UnderwritingNotes { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public UnderwritingDecision? UnderwritingDecision { get; set; }
    public Policy? Policy { get; set; }
}
```

### 2.3 UnderwritingDecision

```csharp
public class UnderwritingDecision
{
    public int UWId { get; set; }
    public int QuoteId { get; set; }
    public int UnderwriterId { get; set; }
    public DateTime DecisionDate { get; set; }
    public string Decision { get; set; } = null!;
    // Valid: Approved, Declined, ReferToSenior, RequestMoreInfo
    public decimal RiskScore { get; set; }

    // Risk Assessment
    public string? CatastropheZoneRating { get; set; }
    public string? ConstructionRating { get; set; }
    public string? OccupancyRating { get; set; }
    public string? ProtectionRating { get; set; }
    public string? LossHistoryRating { get; set; }

    // Catastrophe Exposure
    public bool HighCatExposure { get; set; }
    public decimal? CatastrophePML { get; set; }

    // Conditions
    public string? ApprovalConditions { get; set; }
    public string? DeclineReason { get; set; }
    public string? AdditionalInformationRequired { get; set; }

    // Referral
    public bool ReferredToSeniorUnderwriter { get; set; }
    public int? SeniorUnderwriterId { get; set; }
    public string? ReferralReason { get; set; }

    public string? UnderwritingNotes { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation
    public Quote Quote { get; set; } = null!;
}
```

### 2.4 Policy

```csharp
public class Policy
{
    public int PolicyId { get; set; }
    public int QuoteId { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = "Active";
    // Valid: Active, Cancelled, Expired, PendingCancellation, Renewed

    // Premium and Payment
    public decimal AnnualPremium { get; set; }
    public string PaymentPlan { get; set; } = "Annual";
    // Valid: Annual, SemiAnnual, Quarterly, Monthly
    public decimal? InstallmentAmount { get; set; }
    public DateTime? NextPaymentDue { get; set; }

    // Coverage
    public decimal CoverageLimit { get; set; }
    public decimal Deductible { get; set; }
    public string CoverageType { get; set; } = "Commercial Property";

    // Additional Coverages
    public bool BusinessInterruptionCoverage { get; set; }
    public decimal? BusinessInterruptionLimit { get; set; }
    public bool EquipmentBreakdownCoverage { get; set; }
    public bool FloodCoverage { get; set; }
    public decimal? FloodLimit { get; set; }
    public bool EarthquakeCoverage { get; set; }
    public decimal? EarthquakeLimit { get; set; }

    // Reinsurance
    public bool ReinsuranceCeded { get; set; }
    public decimal? CededPremium { get; set; }
    public string? ReinsuranceTreatyId { get; set; }

    // Cancellation
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    public decimal? ReturnPremium { get; set; }

    // Documents
    public string? PolicyDocumentPath { get; set; }
    public DateTime? DocumentGeneratedDate { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public Quote Quote { get; set; } = null!;
    public ICollection<Endorsement> Endorsements { get; set; } = new List<Endorsement>();
}
```

### 2.5 Endorsement

```csharp
public class Endorsement
{
    public int EndorsementId { get; set; }
    public int PolicyId { get; set; }
    public string EndorsementNumber { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public DateTime RequestDate { get; set; }
    public string EndorsementType { get; set; } = null!;
    // Valid: CoverageChange, PremiumAdjustment, NameChange, AddressChange, Cancellation
    public string Status { get; set; } = "Pending";

    // Changes
    public string? ChangeDescription { get; set; }
    public decimal? PremiumChange { get; set; }
    public decimal? NewCoverageLimit { get; set; }
    public decimal? NewDeductible { get; set; }

    // Approval
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovalDate { get; set; }

    // Documents
    public string? EndorsementDocumentPath { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }

    // Navigation
    public Policy Policy { get; set; } = null!;
}
```

### 2.6 RateFactor

```csharp
public class RateFactor
{
    public int FactorId { get; set; }
    public string FactorType { get; set; } = null!;
    public string FactorCode { get; set; } = null!;
    public string? Description { get; set; }
    public decimal FactorValue { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? StateCode { get; set; }
    public string? TerritoryCode { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
}
```

### 2.7 CoverageOption

```csharp
public class CoverageOption
{
    public int CoverageId { get; set; }
    public string CoverageType { get; set; } = null!;
    public string CoverageCode { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsOptional { get; set; } = true;
    public decimal? BaseRate { get; set; }
    public string? RatingBasis { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 2.8 AuditLog

```csharp
public class AuditLog
{
    public long AuditId { get; set; }
    public string TableName { get; set; } = null!;
    public int RecordId { get; set; }
    public string Action { get; set; } = null!; // INSERT, UPDATE, DELETE
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedBy { get; set; }
    public DateTime ChangedDate { get; set; }
}
```

---

## 3. DbContext Configuration

```csharp
public class KeystoneDbContext : DbContext
{
    public KeystoneDbContext(DbContextOptions<KeystoneDbContext> options)
        : base(options) { }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<UnderwritingDecision> UnderwritingDecisions => Set<UnderwritingDecision>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Endorsement> Endorsements => Set<Endorsement>();
    public DbSet<RateFactor> RateFactors => Set<RateFactor>();
    public DbSet<CoverageOption> CoverageOptions => Set<CoverageOption>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KeystoneDbContext).Assembly);
    }
}
```

---

## 4. Fluent API Configurations

### ClientConfiguration

```csharp
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.ClientId);

        builder.Property(c => c.ClientNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(c => c.ClientNumber).IsUnique();

        builder.Property(c => c.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ContactFirstName).HasMaxLength(100);
        builder.Property(c => c.ContactLastName).HasMaxLength(100);
        builder.Property(c => c.Email).HasMaxLength(100);
        builder.Property(c => c.Phone).HasMaxLength(20);
        builder.Property(c => c.BusinessType).HasMaxLength(100);
        builder.Property(c => c.FederalTaxId).HasMaxLength(11);

        builder.Property(c => c.MailingAddress).HasMaxLength(200);
        builder.Property(c => c.MailingCity).HasMaxLength(100);
        builder.Property(c => c.MailingState).HasMaxLength(2);
        builder.Property(c => c.MailingZip).HasMaxLength(10);

        builder.Property(c => c.AccountStatus).HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(c => c.AccountCreatedDate).HasDefaultValueSql("GETDATE()");
        builder.Property(c => c.CreditScore).HasPrecision(5, 2);
        builder.Property(c => c.RiskTier).HasMaxLength(20);
        builder.Property(c => c.TotalPremiumInForce).HasPrecision(15, 2);

        builder.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasMany(c => c.Quotes)
            .WithOne(q => q.Client)
            .HasForeignKey(q => q.ClientId);
    }
}
```

### QuoteConfiguration

```csharp
public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.HasKey(q => q.QuoteId);

        builder.Property(q => q.QuoteNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(q => q.QuoteNumber).IsUnique();

        builder.Property(q => q.Status).HasMaxLength(20).IsRequired();
        builder.Property(q => q.PropertyAddress).HasMaxLength(200).IsRequired();
        builder.Property(q => q.City).HasMaxLength(100).IsRequired();
        builder.Property(q => q.StateCode).HasMaxLength(2).IsRequired();
        builder.Property(q => q.ZipCode).HasMaxLength(10).IsRequired();
        builder.Property(q => q.CountyName).HasMaxLength(100);
        builder.Property(q => q.PropertyValue).HasPrecision(15, 2);
        builder.Property(q => q.ConstructionType).HasMaxLength(50).IsRequired();
        builder.Property(q => q.OccupancyType).HasMaxLength(100).IsRequired();
        builder.Property(q => q.RoofType).HasMaxLength(50);

        builder.Property(q => q.CoverageLimit).HasPrecision(15, 2);
        builder.Property(q => q.Deductible).HasPrecision(15, 2);
        builder.Property(q => q.BusinessInterruptionLimit).HasPrecision(15, 2);
        builder.Property(q => q.PriorClaimsTotalAmount).HasPrecision(15, 2);

        builder.Property(q => q.BasePremium).HasPrecision(10, 2);
        builder.Property(q => q.TotalPremium).HasPrecision(10, 2);

        builder.Property(q => q.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(q => q.ClientId);
        builder.HasIndex(q => q.Status);
        builder.HasIndex(q => q.StateCode);

        builder.HasOne(q => q.UnderwritingDecision)
            .WithOne(u => u.Quote)
            .HasForeignKey<UnderwritingDecision>(u => u.QuoteId);
    }
}
```

### PolicyConfiguration

```csharp
public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.HasKey(p => p.PolicyId);

        builder.Property(p => p.PolicyNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.PolicyNumber).IsUnique();

        builder.Property(p => p.Status).HasMaxLength(30).IsRequired();
        builder.Property(p => p.AnnualPremium).HasPrecision(10, 2);
        builder.Property(p => p.PaymentPlan).HasMaxLength(20).IsRequired();
        builder.Property(p => p.InstallmentAmount).HasPrecision(10, 2);
        builder.Property(p => p.CoverageLimit).HasPrecision(15, 2);
        builder.Property(p => p.Deductible).HasPrecision(15, 2);
        builder.Property(p => p.CoverageType).HasMaxLength(50).HasDefaultValue("Commercial Property");

        builder.Property(p => p.BusinessInterruptionLimit).HasPrecision(15, 2);
        builder.Property(p => p.FloodLimit).HasPrecision(15, 2);
        builder.Property(p => p.EarthquakeLimit).HasPrecision(15, 2);
        builder.Property(p => p.CededPremium).HasPrecision(10, 2);
        builder.Property(p => p.ReinsuranceTreatyId).HasMaxLength(50);
        builder.Property(p => p.CancellationReason).HasMaxLength(500);
        builder.Property(p => p.ReturnPremium).HasPrecision(10, 2);
        builder.Property(p => p.PolicyDocumentPath).HasMaxLength(500);

        builder.Property(p => p.IssueDate).HasDefaultValueSql("GETDATE()");
        builder.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(p => p.QuoteId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.EffectiveDate);

        builder.HasOne(p => p.Quote)
            .WithOne(q => q.Policy)
            .HasForeignKey<Policy>(p => p.QuoteId);

        builder.HasMany(p => p.Endorsements)
            .WithOne(e => e.Policy)
            .HasForeignKey(e => e.PolicyId);
    }
}
```

### EndorsementConfiguration

```csharp
public class EndorsementConfiguration : IEntityTypeConfiguration<Endorsement>
{
    public void Configure(EntityTypeBuilder<Endorsement> builder)
    {
        builder.ToTable("Endorsements");
        builder.HasKey(e => e.EndorsementId);

        builder.Property(e => e.EndorsementNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.EndorsementNumber).IsUnique();

        builder.Property(e => e.EndorsementType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
        builder.Property(e => e.PremiumChange).HasPrecision(10, 2);
        builder.Property(e => e.NewCoverageLimit).HasPrecision(15, 2);
        builder.Property(e => e.NewDeductible).HasPrecision(15, 2);
        builder.Property(e => e.EndorsementDocumentPath).HasMaxLength(500);

        builder.Property(e => e.RequestDate).HasDefaultValueSql("GETDATE()");
        builder.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");

        builder.HasIndex(e => e.PolicyId);
    }
}
```

---

## 5. Stored Procedure Migration Map

All stored procedures are replaced with EF Core LINQ queries:

| Stored Procedure | Replacement |
|---|---|
| `usp_GetQuoteDetails` | `dbContext.Quotes.Include(q => q.Client).Include(q => q.UnderwritingDecision).FirstOrDefaultAsync(q => q.QuoteId == id)` |
| `usp_CalculatePremium` | `QuotingEngine.CalculatePremium()` — pure C# |
| `usp_SearchQuotes` | Dynamic `IQueryable<Quote>` with conditional `.Where()` filters |
| `usp_GetExpiringPolicies` | `dbContext.Policies.Where(p => p.Status == "Active" && p.ExpirationDate <= cutoff)` |
| `usp_UpdatePolicyStatus` | `policy.Status = newStatus; await dbContext.SaveChangesAsync()` + `AuditInterceptor` |
| `usp_GetPremiumSummaryByState` | `dbContext.Quotes.Where(...).GroupBy(q => q.StateCode).Select(...)` |

---

## 6. Indexes

Indexes are defined in Fluent API configurations above. Summary:

| Table | Index | Columns |
|---|---|---|
| Clients | `IX_Clients_ClientNumber` | `ClientNumber` (unique) |
| Quotes | `IX_Quotes_ClientId` | `ClientId` |
| Quotes | `IX_Quotes_Status` | `Status` |
| Quotes | `IX_Quotes_StateCode` | `StateCode` |
| Quotes | `IX_Quotes_QuoteNumber` | `QuoteNumber` (unique) |
| UnderwritingDecisions | `IX_UW_QuoteId` | `QuoteId` |
| Policies | `IX_Policies_PolicyNumber` | `PolicyNumber` (unique) |
| Policies | `IX_Policies_QuoteId` | `QuoteId` |
| Policies | `IX_Policies_Status` | `Status` |
| Policies | `IX_Policies_EffectiveDate` | `EffectiveDate` |
| Endorsements | `IX_Endorsements_PolicyId` | `PolicyId` |
| Endorsements | `IX_Endorsements_EndorsementNumber` | `EndorsementNumber` (unique) |
| RateFactors | `IX_RateFactors_Type` | `FactorType, StateCode` |
| RateFactors | `IX_RateFactors_Effective` | `EffectiveDate, ExpirationDate` |
| AuditLog | `IX_Audit_Table` | `TableName, RecordId` |
| AuditLog | `IX_Audit_Date` | `ChangedDate` |

---

## 7. Audit Interceptor

Replaces the legacy `AuditLog` INSERT in `usp_UpdatePolicyStatus`:

```csharp
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Modified or EntityState.Added or EntityState.Deleted))
        {
            // Build AuditLog entries for each changed property
        }

        context.Set<AuditLog>().AddRange(auditEntries);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```
