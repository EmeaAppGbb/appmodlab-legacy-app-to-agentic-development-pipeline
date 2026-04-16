using Azure.Messaging.ServiceBus;
using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Publishers;

public class PolicyIssuancePublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<PolicyIssuancePublisher> _logger;

    public PolicyIssuancePublisher(ServiceBusClient client, ILogger<PolicyIssuancePublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task PublishAsync(PolicyIssuedMessage message, CancellationToken ct = default)
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
}
