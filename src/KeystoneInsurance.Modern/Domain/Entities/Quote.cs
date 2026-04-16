namespace KeystoneInsurance.Modern.Domain.Entities;

public class Quote
{
    public int QuoteId { get; set; }
    public int ClientId { get; set; }
    public string QuoteNumber { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string Status { get; set; } = "Draft";

    // Property Information
    public string PropertyAddress { get; set; } = null!;
    public string City { get; set; } = null!;
    public string StateCode { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string? CountyName { get; set; }
    public decimal PropertyValue { get; set; }
    public string ConstructionType { get; set; } = null!;
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
