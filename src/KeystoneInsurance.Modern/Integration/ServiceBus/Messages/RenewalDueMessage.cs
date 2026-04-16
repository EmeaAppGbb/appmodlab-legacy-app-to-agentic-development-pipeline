namespace KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

public record RenewalDueMessage
{
    public int PolicyId { get; init; }
    public string PolicyNumber { get; init; } = null!;
    public DateTime ExpirationDate { get; init; }
    public decimal CurrentPremium { get; init; }
    public int ClientId { get; init; }
    public string StateCode { get; init; } = null!;
}
