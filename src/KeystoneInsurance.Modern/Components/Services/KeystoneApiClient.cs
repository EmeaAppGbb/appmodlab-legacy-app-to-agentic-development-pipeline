using System.Net.Http.Json;

namespace KeystoneInsurance.Modern.Components.Services;

public class KeystoneApiClient
{
    private readonly HttpClient _http;

    public KeystoneApiClient(HttpClient http)
    {
        _http = http;
    }

    // --- Quotes ---

    public async Task<QuoteListResponse> SearchQuotesAsync(
        string? status = null, string? stateCode = null, int page = 1, int pageSize = 20)
    {
        var url = $"api/v1/quotes?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
        if (!string.IsNullOrEmpty(stateCode)) url += $"&stateCode={stateCode}";

        return await _http.GetFromJsonAsync<QuoteListResponse>(url) ?? new();
    }

    public async Task<QuoteDetailDto?> GetQuoteAsync(int quoteId)
    {
        return await _http.GetFromJsonAsync<QuoteDetailDto>($"api/v1/quotes/{quoteId}");
    }

    public async Task<HttpResponseMessage> CreateQuoteAsync(CreateQuoteModel model)
    {
        return await _http.PostAsJsonAsync("api/v1/quotes", model);
    }

    public async Task<HttpResponseMessage> RecalculateQuoteAsync(int quoteId)
    {
        return await _http.PutAsJsonAsync($"api/v1/quotes/{quoteId}/recalculate", new { });
    }

    // --- Underwriting ---

    public async Task<HttpResponseMessage> EvaluateUnderwritingAsync(int quoteId)
    {
        return await _http.PostAsJsonAsync("api/v1/underwriting/evaluate", new { quoteId });
    }

    // --- Policies ---

    public async Task<PolicyDetailDto?> GetPolicyAsync(int policyId)
    {
        return await _http.GetFromJsonAsync<PolicyDetailDto>($"api/v1/policies/{policyId}");
    }

    public async Task<HttpResponseMessage> IssuePolicyAsync(int quoteId, DateTime effectiveDate, string paymentPlan)
    {
        return await _http.PostAsJsonAsync("api/v1/policies", new { quoteId, effectiveDate, paymentPlan });
    }
}

// --- DTOs ---

public class QuoteListResponse
{
    public List<QuoteListItem> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
}

public class QuoteListItem
{
    public int QuoteId { get; set; }
    public string QuoteNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public string PropertyAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string StateCode { get; set; } = "";
    public decimal? TotalPremium { get; set; }
    public string? ClientName { get; set; }
}

public class QuoteDetailDto
{
    public int QuoteId { get; set; }
    public string QuoteNumber { get; set; } = "";
    public int ClientId { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime ExpirationDate { get; set; }

    // Property
    public string PropertyAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string StateCode { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public decimal PropertyValue { get; set; }
    public string ConstructionType { get; set; } = "";
    public string OccupancyType { get; set; } = "";
    public int YearBuilt { get; set; }
    public int SquareFootage { get; set; }
    public int NumberOfStories { get; set; }
    public string? RoofType { get; set; }
    public int RoofAge { get; set; }

    // Protection
    public bool SprinklersInstalled { get; set; }
    public bool AlarmSystemInstalled { get; set; }

    // Coverage
    public decimal CoverageLimit { get; set; }
    public decimal Deductible { get; set; }
    public bool BusinessInterruptionCoverage { get; set; }
    public decimal? BusinessInterruptionLimit { get; set; }
    public bool EquipmentBreakdownCoverage { get; set; }
    public bool FloodCoverage { get; set; }
    public bool EarthquakeCoverage { get; set; }

    // Loss History
    public int PriorClaimsCount { get; set; }
    public decimal PriorClaimsTotalAmount { get; set; }

    // Premium
    public decimal? BasePremium { get; set; }
    public decimal? TotalPremium { get; set; }
    public string? PremiumCalculationDetails { get; set; }

    // Underwriting
    public UnderwritingDecisionDto? UnderwritingDecision { get; set; }
}

public class UnderwritingDecisionDto
{
    public int UWId { get; set; }
    public int QuoteId { get; set; }
    public DateTime DecisionDate { get; set; }
    public string Decision { get; set; } = "";
    public decimal RiskScore { get; set; }
    public string? ConstructionRating { get; set; }
    public string? OccupancyRating { get; set; }
    public string? ProtectionRating { get; set; }
    public string? LossHistoryRating { get; set; }
    public string? CatastropheZoneRating { get; set; }
    public bool HighCatExposure { get; set; }
    public decimal? CatastrophePML { get; set; }
    public string? ApprovalConditions { get; set; }
    public string? DeclineReason { get; set; }
    public bool ReferredToSeniorUnderwriter { get; set; }
    public string? ReferralReason { get; set; }
    public string? AdditionalInformationRequired { get; set; }
    public string? UnderwritingNotes { get; set; }
}

public class PolicyDetailDto
{
    public int PolicyId { get; set; }
    public string PolicyNumber { get; set; } = "";
    public int QuoteId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = "";
    public decimal AnnualPremium { get; set; }
    public string PaymentPlan { get; set; } = "";
    public decimal? InstallmentAmount { get; set; }
    public DateTime? NextPaymentDue { get; set; }
    public decimal CoverageLimit { get; set; }
    public decimal Deductible { get; set; }
    public string CoverageType { get; set; } = "";
    public bool BusinessInterruptionCoverage { get; set; }
    public decimal? BusinessInterruptionLimit { get; set; }
    public bool EquipmentBreakdownCoverage { get; set; }
    public bool FloodCoverage { get; set; }
    public decimal? FloodLimit { get; set; }
    public bool EarthquakeCoverage { get; set; }
    public decimal? EarthquakeLimit { get; set; }
    public bool ReinsuranceCeded { get; set; }
    public decimal? CededPremium { get; set; }
    public string? ReinsuranceTreatyId { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    public decimal? ReturnPremium { get; set; }
}

public class CreateQuoteModel
{
    public int ClientId { get; set; } = 1;
    public string PropertyAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string StateCode { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public decimal PropertyValue { get; set; }
    public string ConstructionType { get; set; } = "Non-Combustible";
    public string OccupancyType { get; set; } = "Manufacturing-Light";
    public int YearBuilt { get; set; } = 2010;
    public int SquareFootage { get; set; }
    public int NumberOfStories { get; set; } = 1;
    public bool SprinklersInstalled { get; set; }
    public bool AlarmSystemInstalled { get; set; }
    public string? RoofType { get; set; } = "TPO/EPDM";
    public int RoofAge { get; set; }
    public decimal CoverageLimit { get; set; }
    public decimal Deductible { get; set; } = 25000;
    public bool BusinessInterruptionCoverage { get; set; }
    public decimal? BusinessInterruptionLimit { get; set; }
    public bool EquipmentBreakdownCoverage { get; set; }
    public bool FloodCoverage { get; set; }
    public bool EarthquakeCoverage { get; set; }
    public int PriorClaimsCount { get; set; }
    public decimal PriorClaimsTotalAmount { get; set; }
}

public class CreateQuoteResponse
{
    public int QuoteId { get; set; }
    public string QuoteNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal? BasePremium { get; set; }
    public decimal? TotalPremium { get; set; }
    public string? PremiumCalculationDetails { get; set; }
}

public class UnderwritingEvaluateResponse
{
    public int UWId { get; set; }
    public int QuoteId { get; set; }
    public DateTime DecisionDate { get; set; }
    public string Decision { get; set; } = "";
    public decimal RiskScore { get; set; }
    public RatingsDto? Ratings { get; set; }
    public CatastropheExposureDto? CatastropheExposure { get; set; }
    public string? ApprovalConditions { get; set; }
    public string? DeclineReason { get; set; }
    public bool ReferredToSeniorUnderwriter { get; set; }
    public string? ReferralReason { get; set; }
    public string? AdditionalInformationRequired { get; set; }
    public string? Notes { get; set; }
}

public class RatingsDto
{
    public string? ConstructionRating { get; set; }
    public string? OccupancyRating { get; set; }
    public string? ProtectionRating { get; set; }
    public string? LossHistoryRating { get; set; }
    public string? CatastropheZoneRating { get; set; }
}

public class CatastropheExposureDto
{
    public bool HighCatExposure { get; set; }
    public decimal? CatastrophePML { get; set; }
}
