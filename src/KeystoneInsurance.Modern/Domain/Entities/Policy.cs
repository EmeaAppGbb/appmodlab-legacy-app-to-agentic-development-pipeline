namespace KeystoneInsurance.Modern.Domain.Entities;

public class Policy
{
    public int PolicyId { get; set; }
    public int QuoteId { get; set; }
    public string PolicyNumber { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = "Active";

    // Premium and Payment
    public decimal AnnualPremium { get; set; }
    public string PaymentPlan { get; set; } = "Annual";
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
