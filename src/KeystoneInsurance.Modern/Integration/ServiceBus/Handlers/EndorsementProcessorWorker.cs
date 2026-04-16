using Azure.Messaging.ServiceBus;
using KeystoneInsurance.Modern.Documents;
using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Handlers;

/// <summary>
/// Background worker that processes endorsement-requested messages from Azure Service Bus
/// and triggers endorsement document generation.
/// </summary>
public class EndorsementProcessorWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EndorsementProcessorWorker> _logger;

    public EndorsementProcessorWorker(
        ServiceBusClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<EndorsementProcessorWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor("endorsement-requested", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
        });

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var message = args.Message.Body.ToObjectFromJson<EndorsementRequestedMessage>()
                    ?? throw new InvalidOperationException("Failed to deserialize EndorsementRequestedMessage");
                _logger.LogInformation(
                    "Processing EndorsementRequested for {EndorsementNumber} (EndorsementId: {EndorsementId})",
                    message.EndorsementNumber, message.EndorsementId);

                using var scope = _scopeFactory.CreateScope();
                var documentGenerator = scope.ServiceProvider
                    .GetRequiredService<IPolicyDocumentGenerator>();

                await documentGenerator.GenerateEndorsementDocumentAsync(message.EndorsementId);
                await args.CompleteMessageAsync(args.Message, stoppingToken);

                _logger.LogInformation(
                    "Completed endorsement document generation for {EndorsementNumber}",
                    message.EndorsementNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process EndorsementRequested message {MessageId}", args.Message.MessageId);
                throw;
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception,
                "Error processing endorsement-requested message: {Source}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
