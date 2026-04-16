using System.Net.Http.Json;

namespace KeystoneInsurance.Modern.Integration.Reinsurance;

/// <summary>
/// HTTP-based reinsurance API client replacing legacy WCF BasicHttpBinding client.
/// Uses IHttpClientFactory with Polly resilience pipelines (retry + circuit breaker + timeout).
/// </summary>
public class ReinsuranceApiClient : IReinsuranceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReinsuranceApiClient> _logger;

    public ReinsuranceApiClient(HttpClient httpClient, ILogger<ReinsuranceApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ReinsuranceCessionResponse> CedeRiskAsync(
        ReinsuranceCessionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Ceding risk for Policy {PolicyNumber}, Treaty {TreatyId}, Amount {CededPremium:C}",
            request.PolicyNumber, request.TreatyId, request.CededPremium);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/cessions", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content
            .ReadFromJsonAsync<ReinsuranceCessionResponse>(cancellationToken))!;

        _logger.LogInformation(
            "Cession {CessionId} accepted at {Percentage}% for Policy {PolicyNumber}",
            result.CessionId, result.AcceptedPercentage, request.PolicyNumber);

        return result;
    }

    public async Task<ReinsuranceRecoveryResponse> SubmitClaimAsync(
        ReinsuranceClaimRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Submitting reinsurance claim for Policy {PolicyNumber}, Cession {CessionId}, Amount {ClaimAmount:C}",
            request.PolicyNumber, request.CessionId, request.ClaimAmount);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/claims", request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content
            .ReadFromJsonAsync<ReinsuranceRecoveryResponse>(cancellationToken))!;

        _logger.LogInformation(
            "Recovery {RecoveryId} processed, recoverable amount {Amount:C}",
            result.RecoveryId, result.RecoverableAmount);

        return result;
    }
}
