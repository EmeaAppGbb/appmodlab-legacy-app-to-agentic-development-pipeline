using Azure.Messaging.ServiceBus;
using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Publishers;

/// <summary>
/// Unified publisher implementing IMessagePublisher for all Service Bus message types.
/// Delegates to queue-specific senders for policy-issued, endorsement-requested,
/// renewal-due queues and the compliance-events topic.
/// </summary>
public class ServiceBusPublisher : IMessagePublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<ServiceBusPublisher> _logger;

    public ServiceBusPublisher(ServiceBusClient client, ILogger<ServiceBusPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task PublishPolicyIssuedAsync(PolicyIssuedMessage message, CancellationToken ct = default)
    {
        await using var sender = _client.CreateSender("policy-issued");

        var sbMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = "PolicyIssued",
            MessageId = $"policy-{message.PolicyId}-{message.IssuedAt:yyyyMMddHHmmss}",
            ApplicationProperties =
            {
                ["StateCode"] = message.StateCode,
                ["PolicyNumber"] = message.PolicyNumber
            }
        };

        await sender.SendMessageAsync(sbMessage, ct);
        _logger.LogInformation("Published PolicyIssued for {PolicyNumber}", message.PolicyNumber);
    }

    public async Task PublishEndorsementRequestedAsync(EndorsementRequestedMessage message, CancellationToken ct = default)
    {
        await using var sender = _client.CreateSender("endorsement-requested");

        var sbMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = "EndorsementRequested",
            MessageId = $"endorsement-{message.EndorsementId}-{message.RequestedAt:yyyyMMddHHmmss}",
            ApplicationProperties =
            {
                ["EndorsementType"] = message.EndorsementType,
                ["PolicyId"] = message.PolicyId
            }
        };

        await sender.SendMessageAsync(sbMessage, ct);
        _logger.LogInformation(
            "Published EndorsementRequested for {EndorsementNumber}", message.EndorsementNumber);
    }

    public async Task PublishRenewalDueAsync(RenewalDueMessage message, CancellationToken ct = default)
    {
        await using var sender = _client.CreateSender("renewal-due");

        var sbMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = "RenewalDue",
            MessageId = $"renewal-{message.PolicyId}-{message.ExpirationDate:yyyyMMdd}",
            ApplicationProperties =
            {
                ["StateCode"] = message.StateCode,
                ["PolicyNumber"] = message.PolicyNumber
            }
        };

        await sender.SendMessageAsync(sbMessage, ct);
        _logger.LogInformation(
            "Published RenewalDue for {PolicyNumber}, expires {ExpirationDate:d}",
            message.PolicyNumber, message.ExpirationDate);
    }

    public async Task PublishComplianceEventAsync(ComplianceEventMessage message, CancellationToken ct = default)
    {
        await using var sender = _client.CreateSender("compliance-events");

        var sbMessage = new ServiceBusMessage(BinaryData.FromObjectAsJson(message))
        {
            ContentType = "application/json",
            Subject = message.EventType,
            MessageId = $"compliance-{message.EventType}-{message.OccurredAt:yyyyMMddHHmmss}",
            ApplicationProperties =
            {
                ["EventType"] = message.EventType,
                ["StateCode"] = message.StateCode
            }
        };

        await sender.SendMessageAsync(sbMessage, ct);
        _logger.LogInformation(
            "Published ComplianceEvent {EventType} for state {StateCode}",
            message.EventType, message.StateCode);
    }
}
