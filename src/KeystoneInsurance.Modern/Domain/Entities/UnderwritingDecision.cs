namespace KeystoneInsurance.Modern.Domain.Entities;

public class UnderwritingDecision
{
    public int UWId { get; set; }
    public int QuoteId { get; set; }
    public int UnderwriterId { get; set; }
    public DateTime DecisionDate { get; set; }
    public string Decision { get; set; } = null!;
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
