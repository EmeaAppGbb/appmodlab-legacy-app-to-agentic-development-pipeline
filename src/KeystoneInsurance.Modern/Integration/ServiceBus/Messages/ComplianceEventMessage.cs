namespace KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

public record ComplianceEventMessage
{
    public string EventType { get; init; } = null!;
    public string StateCode { get; init; } = null!;
    public int? PolicyId { get; init; }
    public string? PolicyNumber { get; init; }
    public string Description { get; init; } = null!;
    public DateTime OccurredAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}
