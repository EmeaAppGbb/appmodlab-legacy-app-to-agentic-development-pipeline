using Azure.Messaging.ServiceBus;
using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Handlers;

/// <summary>
/// Background worker that processes renewal-due messages from Azure Service Bus
/// and triggers renewal notification generation.
/// </summary>
public class RenewalProcessorWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RenewalProcessorWorker> _logger;

    public RenewalProcessorWorker(
        ServiceBusClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<RenewalProcessorWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor("renewal-due", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 3,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
        });

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var message = args.Message.Body.ToObjectFromJson<RenewalDueMessage>()
                    ?? throw new InvalidOperationException("Failed to deserialize RenewalDueMessage");
                _logger.LogInformation(
                    "Processing RenewalDue for {PolicyNumber} (PolicyId: {PolicyId}), expires {ExpirationDate:d}",
                    message.PolicyNumber, message.PolicyId, message.ExpirationDate);

                using var scope = _scopeFactory.CreateScope();
                var documentGenerator = scope.ServiceProvider
                    .GetRequiredService<Documents.IPolicyDocumentGenerator>();

                await documentGenerator.GenerateRenewalOfferAsync(message.PolicyId);
                await args.CompleteMessageAsync(args.Message, stoppingToken);

                _logger.LogInformation(
                    "Completed renewal processing for {PolicyNumber}", message.PolicyNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process RenewalDue message {MessageId}", args.Message.MessageId);
                throw;
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception,
                "Error processing renewal-due message: {Source}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
