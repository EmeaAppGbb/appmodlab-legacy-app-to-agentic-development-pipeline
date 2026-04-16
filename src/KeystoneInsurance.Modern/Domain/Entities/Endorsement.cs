namespace KeystoneInsurance.Modern.Domain.Entities;

public class Endorsement
{
    public int EndorsementId { get; set; }
    public int PolicyId { get; set; }
    public string EndorsementNumber { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public DateTime RequestDate { get; set; }
    public string EndorsementType { get; set; } = null!;
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
