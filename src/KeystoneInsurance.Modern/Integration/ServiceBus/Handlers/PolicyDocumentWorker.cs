using Azure.Messaging.ServiceBus;
using KeystoneInsurance.Modern.Documents;
using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Handlers;

/// <summary>
/// Background worker that processes policy-issued messages from Azure Service Bus
/// and triggers PDF document generation.
/// </summary>
public class PolicyDocumentWorker : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PolicyDocumentWorker> _logger;

    public PolicyDocumentWorker(
        ServiceBusClient client,
        IServiceScopeFactory scopeFactory,
        ILogger<PolicyDocumentWorker> logger)
    {
        _client = client;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processor = _client.CreateProcessor("policy-issued", new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 5,
            AutoCompleteMessages = false,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(10)
        });

        processor.ProcessMessageAsync += async args =>
        {
            try
            {
                var message = args.Message.Body.ToObjectFromJson<PolicyIssuedMessage>()
                    ?? throw new InvalidOperationException("Failed to deserialize PolicyIssuedMessage");
                _logger.LogInformation(
                    "Processing PolicyIssued for {PolicyNumber} (PolicyId: {PolicyId})",
                    message.PolicyNumber, message.PolicyId);

                using var scope = _scopeFactory.CreateScope();
                var documentGenerator = scope.ServiceProvider
                    .GetRequiredService<IPolicyDocumentGenerator>();

                await documentGenerator.GeneratePolicyDocumentAsync(message.PolicyId);
                await args.CompleteMessageAsync(args.Message, stoppingToken);

                _logger.LogInformation(
                    "Completed policy document generation for {PolicyNumber}", message.PolicyNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process PolicyIssued message {MessageId}", args.Message.MessageId);
                throw; // Let Service Bus handle retry via delivery count
            }
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception,
                "Error processing policy-issued message: {Source}", args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
