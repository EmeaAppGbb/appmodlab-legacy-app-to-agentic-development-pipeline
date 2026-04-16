namespace KeystoneInsurance.Modern.Documents;

public interface IPolicyDocumentGenerator
{
    Task<DocumentResult> GeneratePolicyDocumentAsync(int policyId);
    Task<DocumentResult> GenerateEndorsementDocumentAsync(int endorsementId);
    Task<DocumentResult> GenerateQuoteSummaryAsync(int quoteId);
    Task<DocumentResult> GenerateRenewalOfferAsync(int renewalPolicyId);
}

public record DocumentResult
{
    public string BlobUrl { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public long FileSizeBytes { get; init; }
    public DateTime GeneratedAt { get; init; }
}
