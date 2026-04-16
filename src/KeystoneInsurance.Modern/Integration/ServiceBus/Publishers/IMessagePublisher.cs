using KeystoneInsurance.Modern.Integration.ServiceBus.Messages;

namespace KeystoneInsurance.Modern.Integration.ServiceBus.Publishers;

public interface IMessagePublisher
{
    Task PublishPolicyIssuedAsync(PolicyIssuedMessage message, CancellationToken ct = default);
    Task PublishEndorsementRequestedAsync(EndorsementRequestedMessage message, CancellationToken ct = default);
    Task PublishRenewalDueAsync(RenewalDueMessage message, CancellationToken ct = default);
    Task PublishComplianceEventAsync(ComplianceEventMessage message, CancellationToken ct = default);
}
