namespace KeystoneInsurance.Modern.Domain.Entities;

public class Coverage
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
