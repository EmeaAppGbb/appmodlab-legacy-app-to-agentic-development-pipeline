namespace KeystoneInsurance.Modern.Integration.Regulatory;

public interface IRegulatoryReportingClient
{
    Task<RegulatorySubmissionResponse> SubmitQuarterlyReportAsync(
        QuarterlyReportData data, CancellationToken ct = default);

    Task<RegulatorySubmissionResponse> SubmitRateFilingAsync(
        string stateCode, RateFilingData data, CancellationToken ct = default);

    Task SubmitPolicyTransactionAsync(
        string stateCode, PolicyTransactionData data, CancellationToken ct = default);
}

public record QuarterlyReportData
{
    public int Year { get; init; }
    public int Quarter { get; init; }
    public string StateCode { get; init; } = null!;
    public decimal TotalWrittenPremium { get; init; }
    public decimal TotalEarnedPremium { get; init; }
    public int PoliciesInForce { get; init; }
    public int NewPoliciesWritten { get; init; }
    public int PoliciesCancelled { get; init; }
    public decimal LossesIncurred { get; init; }
    public decimal LossRatio { get; init; }
}

public record RateFilingData
{
    public string FilingType { get; init; } = null!;
    public string CoverageType { get; init; } = null!;
    public decimal ProposedRateChange { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string Justification { get; init; } = null!;
    public string ActuarialCertification { get; init; } = null!;
}

public record PolicyTransactionData
{
    public string TransactionType { get; init; } = null!;
    public string PolicyNumber { get; init; } = null!;
    public DateTime TransactionDate { get; init; }
    public decimal PremiumAmount { get; init; }
    public string CoverageType { get; init; } = null!;
    public string InsuredName { get; init; } = null!;
    public string PropertyAddress { get; init; } = null!;
}

public record RegulatorySubmissionResponse
{
    public string SubmissionId { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string? ConfirmationNumber { get; init; }
    public DateTime ReceivedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
