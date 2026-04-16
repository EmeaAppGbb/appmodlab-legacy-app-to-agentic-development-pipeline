using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Domain.Entities;
using KeystoneInsurance.Modern.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneInsurance.Modern.Services;

public interface IUnderwritingService
{
    Task<UnderwritingDecision> EvaluateAsync(int quoteId, int underwriterId, CancellationToken ct = default);
}

public class UnderwritingService : IUnderwritingService
{
    private readonly KeystoneDbContext _db;
    private readonly ILogger<UnderwritingService> _logger;

    public UnderwritingService(KeystoneDbContext db, ILogger<UnderwritingService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<UnderwritingDecision> EvaluateAsync(int quoteId, int underwriterId, CancellationToken ct = default)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.QuoteId == quoteId, ct)
            ?? throw new NotFoundException($"Quote {quoteId} not found");

        var buildingAge = DateTime.UtcNow.Year - quote.YearBuilt;
        var riskScore = CalculateRiskScore(quote, buildingAge);
        var pml = CalculatePML(quote);
        var highCatExposure = pml > quote.PropertyValue * 0.50m;

        // Auto-decline check
        if (UnderwritingRules.IsAutoDecline(buildingAge, quote.ConstructionType,
            quote.PriorClaimsCount, pml, quote.PropertyValue))
        {
            return await CreateDecision(quote, underwriterId, "Declined", riskScore, pml,
                highCatExposure, buildingAge, "Auto-declined per underwriting rules", ct);
        }

        // Decision thresholds
        string decision;
        string? reason = null;

        if (riskScore > 85 || (highCatExposure && pml > quote.PropertyValue * 0.50m))
        {
            decision = "Declined";
            reason = riskScore > 85 ? $"Risk score {riskScore:F2} exceeds threshold" : "High catastrophe exposure";
        }
        else if (riskScore > 70 || quote.PriorClaimsCount >= 3)
        {
            decision = "ReferToSenior";
            reason = riskScore > 70 ? $"Risk score {riskScore:F2} requires senior review"
                : $"{quote.PriorClaimsCount} prior claims require senior review";
        }
        else if (riskScore > 60 || NeedsMoreInformation(quote))
        {
            decision = "RequestMoreInfo";
            reason = GetMissingInformationDescription(quote);
        }
        else
        {
            decision = "Approved";
        }

        return await CreateDecision(quote, underwriterId, decision, riskScore, pml,
            highCatExposure, buildingAge, reason, ct);
    }

    private decimal CalculateRiskScore(Quote quote, int buildingAge)
    {
        var score = 50m;

        // Construction
        score += quote.ConstructionType switch
        {
            "Frame" => 15, "Joisted Masonry" => 10, "Non-Combustible" => 5, "Fire Resistive" => -5, _ => 0
        };

        // Building age
        score += buildingAge switch
        {
            > 50 => 15, > 30 => 10, > 20 => 5, < 5 => -5, _ => 0
        };

        // Occupancy
        score += quote.OccupancyType switch
        {
            "Restaurant" => 12, "Manufacturing-Heavy" => 15, "Office" => -5, _ => 0
        };

        // Protection
        if (quote.SprinklersInstalled) score -= 10;
        if (quote.AlarmSystemInstalled) score -= 5;

        // Claims
        score += quote.PriorClaimsCount * 8;
        if (quote.PriorClaimsTotalAmount > 100_000) score += 10;

        // High-cat states
        if (quote.StateCode is "FL" or "CA" or "LA" or "TX") score += 10;

        // Roof age
        if (quote.RoofAge > 20) score += 12;
        else if (quote.RoofAge < 5) score -= 3;

        // High value
        if (quote.PropertyValue > 5_000_000) score += 8;

        return Math.Clamp(score, 0, 100);
    }

    private static decimal CalculatePML(Quote quote)
    {
        var pml = quote.StateCode switch
        {
            "FL" or "LA" => quote.PropertyValue * 0.60m,
            "CA" => quote.PropertyValue * 0.50m,
            _ => quote.PropertyValue * 0.25m
        };

        if (quote.SprinklersInstalled)
            pml *= 0.70m;

        return pml;
    }

    private static bool NeedsMoreInformation(Quote quote)
    {
        if (quote.RoofAge > 20 && string.IsNullOrEmpty(quote.RoofType)) return true;
        if (quote.PriorClaimsCount > 0 && quote.PriorClaimsTotalAmount == 0) return true;
        if (quote.PropertyValue > 2_000_000 && quote.SquareFootage == 0) return true;
        return false;
    }

    private static string? GetMissingInformationDescription(Quote quote)
    {
        var items = new List<string>();
        if (quote.RoofAge > 20 && string.IsNullOrEmpty(quote.RoofType))
            items.Add("Roof type required for buildings with roof age > 20 years");
        if (quote.PriorClaimsCount > 0 && quote.PriorClaimsTotalAmount == 0)
            items.Add("Prior claims total amount required when claims exist");
        if (quote.PropertyValue > 2_000_000 && quote.SquareFootage == 0)
            items.Add("Square footage required for properties over $2M");
        return items.Count > 0 ? string.Join("; ", items) : null;
    }

    private async Task<UnderwritingDecision> CreateDecision(
        Quote quote, int underwriterId, string decision, decimal riskScore,
        decimal pml, bool highCatExposure, int buildingAge, string? reason, CancellationToken ct)
    {
        var uw = new UnderwritingDecision
        {
            QuoteId = quote.QuoteId,
            UnderwriterId = underwriterId,
            DecisionDate = DateTime.UtcNow,
            Decision = decision,
            RiskScore = riskScore,
            HighCatExposure = highCatExposure,
            CatastrophePML = pml,
            ConstructionRating = GetConstructionRating(quote.ConstructionType, buildingAge),
            OccupancyRating = GetOccupancyRating(quote.OccupancyType),
            ProtectionRating = GetProtectionRating(quote.SprinklersInstalled, quote.AlarmSystemInstalled),
            LossHistoryRating = GetLossHistoryRating(quote.PriorClaimsCount, quote.PriorClaimsTotalAmount),
            CatastropheZoneRating = GetCatastropheZoneRating(quote.StateCode),
            CreatedDate = DateTime.UtcNow
        };

        // Set decision-specific fields
        switch (decision)
        {
            case "Approved":
                uw.ApprovalConditions = GetApprovalConditions(quote, riskScore);
                break;
            case "Declined":
                uw.DeclineReason = reason;
                break;
            case "ReferToSenior":
                uw.ReferredToSeniorUnderwriter = true;
                uw.ReferralReason = reason;
                break;
            case "RequestMoreInfo":
                uw.AdditionalInformationRequired = reason;
                break;
        }

        uw.UnderwritingNotes = $"Risk Score: {riskScore:F2}. Evaluated {DateTime.UtcNow:G}";

        // Update quote status
        quote.Status = decision == "Approved" ? "Approved" : decision == "Declined" ? "Declined" : "Pending";

        _db.UnderwritingDecisions.Add(uw);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Underwriting decision for Quote {QuoteId}: {Decision} (score: {RiskScore})",
            quote.QuoteId, decision, riskScore);

        return uw;
    }

    private static string GetConstructionRating(string constructionType, int buildingAge)
    {
        if (constructionType == "Fire Resistive" && buildingAge < 20) return "Excellent";
        if (constructionType == "Frame" && buildingAge > 50) return "Poor";
        if (buildingAge < 10) return "Good";
        if (buildingAge > 40) return "Fair";
        return "Average";
    }

    private static string GetOccupancyRating(string occupancyType) => occupancyType switch
    {
        "Office" or "Educational" or "Warehouse" => "Low Risk",
        "Restaurant" or "Manufacturing-Heavy" or "Hotel" => "High Risk",
        _ => "Average Risk"
    };

    private static string GetProtectionRating(bool sprinklers, bool alarm) => (sprinklers, alarm) switch
    {
        (true, true) => "Superior",
        (true, false) or (false, true) => "Good",
        _ => "Basic"
    };

    private static string GetLossHistoryRating(int claimsCount, decimal totalAmount) => claimsCount switch
    {
        0 => "Loss Free",
        1 when totalAmount < 25_000 => "Favorable",
        <= 2 when totalAmount < 100_000 => "Average",
        >= 3 => "Poor",
        _ when totalAmount > 250_000 => "Poor",
        _ => "Below Average"
    };

    private static string GetCatastropheZoneRating(string stateCode) => stateCode switch
    {
        "FL" or "LA" => "Extreme",
        "CA" or "TX" or "NC" or "SC" => "High",
        _ => "Moderate"
    };

    private static string GetApprovalConditions(Quote quote, decimal riskScore)
    {
        var conditions = new List<string>();

        if (quote.RoofAge > 15)
            conditions.Add("Roof inspection within 30 days of binding");
        if (riskScore > 55)
            conditions.Add("Annual property inspections required");
        if (quote.PropertyValue > 1_000_000)
            conditions.Add("Agreed value settlement basis");

        return conditions.Count > 0 ? string.Join("; ", conditions) : "Standard terms apply";
    }
}
