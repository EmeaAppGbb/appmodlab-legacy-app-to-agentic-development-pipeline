namespace KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

public record PolicyIssuedMessage
{
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = null!;
    public int QuoteId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public decimal AnnualPremium { get; init; }
    public string PaymentPlan { get; init; } = null!;
    public string StateCode { get; init; } = null!;
    public DateTime IssuedAt { get; init; }
    public string IssuedBy { get; init; } = null!;
}
