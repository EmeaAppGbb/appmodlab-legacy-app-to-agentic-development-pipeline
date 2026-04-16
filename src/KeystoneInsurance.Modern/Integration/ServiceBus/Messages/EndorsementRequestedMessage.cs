namespace KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

public record EndorsementRequestedMessage
{
    public int EndorsementId { get; init; }
    public string EndorsementNumber { get; init; } = null!;
    public int PolicyId { get; init; }
    public string EndorsementType { get; init; } = null!;
    public DateTime EffectiveDate { get; init; }
    public decimal? PremiumChange { get; init; }
    public DateTime RequestedAt { get; init; }
}
