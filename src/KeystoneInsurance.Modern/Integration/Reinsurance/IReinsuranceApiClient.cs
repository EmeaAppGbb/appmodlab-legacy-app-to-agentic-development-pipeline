namespace KeystoneInsurance.Modern.Integration.Reinsurance;

public interface IReinsuranceApiClient
{
    Task<ReinsuranceCessionResponse> CedeRiskAsync(
        ReinsuranceCessionRequest request,
        CancellationToken cancellationToken = default);

    Task<ReinsuranceRecoveryResponse> SubmitClaimAsync(
        ReinsuranceClaimRequest request,
        CancellationToken cancellationToken = default);
}

public record ReinsuranceCessionRequest
{
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = null!;
    public string TreatyId { get; init; } = null!;
    public decimal CededPremium { get; init; }
    public decimal CessionPercentage { get; init; }
    public decimal CoverageLimit { get; init; }
    public DateTime EffectiveDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string StateCode { get; init; } = null!;
    public string ConstructionType { get; init; } = null!;
    public string OccupancyType { get; init; } = null!;
}

public record ReinsuranceCessionResponse
{
    public string CessionId { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string TreatyId { get; init; } = null!;
    public decimal AcceptedPercentage { get; init; }
    public DateTime ProcessedAt { get; init; }
}

public record ReinsuranceClaimRequest
{
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = null!;
    public string CessionId { get; init; } = null!;
    public decimal ClaimAmount { get; init; }
    public string ClaimDescription { get; init; } = null!;
    public DateTime LossDate { get; init; }
}

public record ReinsuranceRecoveryResponse
{
    public string RecoveryId { get; init; } = null!;
    public string Status { get; init; } = null!;
    public decimal RecoverableAmount { get; init; }
    public DateTime ProcessedAt { get; init; }
}
