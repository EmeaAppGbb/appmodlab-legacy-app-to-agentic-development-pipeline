using Azure.Messaging.ServiceBus;
using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Publishers;

public class EndorsementPublisher
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<EndorsementPublisher> _logger;

    public EndorsementPublisher(ServiceBusClient client, ILogger<EndorsementPublisher> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task PublishAsync(EndorsementRequestedMessage message, CancellationToken ct = default)
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
            "Published EndorsementRequested for {EndorsementNumber} on Policy {PolicyId}",
            message.EndorsementNumber, message.PolicyId);
    }
}
