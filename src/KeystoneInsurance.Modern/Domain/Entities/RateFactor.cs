namespace KeystoneInsurance.Modern.Domain.Entities;

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
