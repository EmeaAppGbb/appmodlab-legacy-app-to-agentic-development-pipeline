using System.Net.Http.Json;

namespace KeystoneInsurance.Modern.Integration.Regulatory;

/// <summary>
/// HTTP-based regulatory reporting client replacing legacy WCF-based NAIC reporting.
/// Uses IHttpClientFactory with Polly resilience pipelines (retry + timeout).
/// </summary>
public class RegulatoryReportingClient : IRegulatoryReportingClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegulatoryReportingClient> _logger;

    public RegulatoryReportingClient(HttpClient httpClient, ILogger<RegulatoryReportingClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RegulatorySubmissionResponse> SubmitQuarterlyReportAsync(
        QuarterlyReportData data, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Submitting quarterly report for {StateCode} Q{Quarter} {Year}",
            data.StateCode, data.Quarter, data.Year);

        var response = await _httpClient.PostAsJsonAsync(
            "/api/v1/reports/quarterly", data, ct);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content
            .ReadFromJsonAsync<RegulatorySubmissionResponse>(ct))!;

        _logger.LogInformation(
            "Quarterly report submitted: {SubmissionId}, Status: {Status}",
            result.SubmissionId, result.Status);

        return result;
    }

    public async Task<RegulatorySubmissionResponse> SubmitRateFilingAsync(
        string stateCode, RateFilingData data, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Submitting rate filing for {StateCode}, Type: {FilingType}, Change: {RateChange:P}",
            stateCode, data.FilingType, data.ProposedRateChange);

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/states/{stateCode}/rate-filings", data, ct);
        response.EnsureSuccessStatusCode();

        var result = (await response.Content
            .ReadFromJsonAsync<RegulatorySubmissionResponse>(ct))!;

        _logger.LogInformation(
            "Rate filing submitted: {SubmissionId}, Confirmation: {Confirmation}",
            result.SubmissionId, result.ConfirmationNumber);

        return result;
    }

    public async Task SubmitPolicyTransactionAsync(
        string stateCode, PolicyTransactionData data, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Submitting policy transaction for {StateCode}, Policy: {PolicyNumber}, Type: {TransactionType}",
            stateCode, data.PolicyNumber, data.TransactionType);

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/v1/states/{stateCode}/transactions", data, ct);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation(
            "Policy transaction submitted for {PolicyNumber} in {StateCode}",
            data.PolicyNumber, stateCode);
    }
}
