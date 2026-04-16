using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneInsurance.Modern.Services;

public interface IQuotingEngine
{
    Task<Quote> CreateQuoteAsync(Quote quote, CancellationToken ct = default);
    Task<Quote> RecalculateAsync(int quoteId, CancellationToken ct = default);
    List<string> ValidateQuote(Quote quote);
}

public class QuotingEngine : IQuotingEngine
{
    private readonly KeystoneDbContext _db;
    private readonly IPremiumCalculator _premiumCalculator;
    private readonly IComplianceService _complianceService;
    private readonly ILogger<QuotingEngine> _logger;

    public QuotingEngine(
        KeystoneDbContext db,
        IPremiumCalculator premiumCalculator,
        IComplianceService complianceService,
        ILogger<QuotingEngine> logger)
    {
        _db = db;
        _premiumCalculator = premiumCalculator;
        _complianceService = complianceService;
        _logger = logger;
    }

    public async Task<Quote> CreateQuoteAsync(Quote quote, CancellationToken ct = default)
    {
        var errors = ValidateQuote(quote);
        if (errors.Count > 0)
            throw new ValidationException(errors);

        quote.QuoteNumber = GenerateQuoteNumber();
        quote.CreatedDate = DateTime.UtcNow;
        quote.ExpirationDate = quote.CreatedDate.AddDays(30);
        quote.Status = "Draft";

        CalculatePremium(quote);

        var complianceResult = _complianceService.ValidateCompliance(quote);
        if (!complianceResult.IsCompliant)
            throw new ComplianceException(complianceResult.Violations);

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created quote {QuoteNumber} with premium {TotalPremium}",
            quote.QuoteNumber, quote.TotalPremium);

        return quote;
    }

    public async Task<Quote> RecalculateAsync(int quoteId, CancellationToken ct = default)
    {
        var quote = await _db.Quotes.FindAsync([quoteId], ct)
            ?? throw new NotFoundException($"Quote {quoteId} not found");

        CalculatePremium(quote);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Recalculated quote {QuoteId}, new premium: {TotalPremium}",
            quoteId, quote.TotalPremium);

        return quote;
    }

    public List<string> ValidateQuote(Quote quote)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(quote.StateCode))
            errors.Add("State code is required");
        if (quote.PropertyValue <= 0)
            errors.Add("Property value must be greater than zero");
        if (quote.CoverageLimit <= 0)
            errors.Add("Coverage limit must be greater than zero");
        if (quote.CoverageLimit > quote.PropertyValue * 1.5m)
            errors.Add("Coverage limit cannot exceed 150% of property value");
        if (quote.Deductible < 500)
            errors.Add("Minimum deductible is $500");
        if (string.IsNullOrWhiteSpace(quote.ConstructionType))
            errors.Add("Construction type is required");
        if (string.IsNullOrWhiteSpace(quote.OccupancyType))
            errors.Add("Occupancy type is required");
        if (quote.YearBuilt < 1800 || quote.YearBuilt > DateTime.UtcNow.Year)
            errors.Add($"Year built must be between 1800 and {DateTime.UtcNow.Year}");
        if (quote.SquareFootage <= 0)
            errors.Add("Square footage must be greater than zero");

        return errors;
    }

    private void CalculatePremium(Quote quote)
    {
        var baseRate = GetBaseRate(quote.StateCode, quote.ConstructionType);
        var propertyValueFactor = _premiumCalculator.GetPropertyValueFactor(quote.PropertyValue);
        var constructionFactor = _premiumCalculator.GetConstructionTypeFactor(quote.ConstructionType);
        var ageFactor = _premiumCalculator.GetBuildingAgeFactor(DateTime.UtcNow.Year - quote.YearBuilt);
        var occupancyFactor = _premiumCalculator.GetOccupancyFactor(quote.OccupancyType, quote.StateCode);
        var protectionFactor = _premiumCalculator.GetProtectionFactor(quote.SprinklersInstalled, quote.AlarmSystemInstalled);
        var territoryFactor = _premiumCalculator.GetTerritoryFactor(quote.ZipCode, quote.StateCode);
        var catFactor = _premiumCalculator.GetCatastropheZoneFactor(quote.StateCode, quote.ZipCode);
        var roofFactor = _premiumCalculator.GetRoofFactor(quote.RoofType, quote.RoofAge);
        var sqftFactor = _premiumCalculator.GetSquareFootageFactor(quote.SquareFootage);
        var storiesFactor = _premiumCalculator.GetStoriesFactor(quote.NumberOfStories);
        var lossFactor = _premiumCalculator.GetLossHistoryFactor(quote.PriorClaimsCount, quote.PriorClaimsTotalAmount);
        var deductibleCredit = _premiumCalculator.GetDeductibleCredit(quote.Deductible, quote.PropertyValue);

        var basePremium = baseRate * propertyValueFactor * constructionFactor * ageFactor
            * occupancyFactor * protectionFactor * territoryFactor * catFactor
            * roofFactor * sqftFactor * storiesFactor * lossFactor * deductibleCredit;

        var optionalCoverages = _premiumCalculator.CalculateOptionalCoverages(quote);
        var stateAdjustment = _premiumCalculator.GetStateAdjustmentFactor(quote.StateCode);
        var surcharges = _premiumCalculator.CalculateSurchargesAndTaxes(quote.StateCode, basePremium + optionalCoverages);

        var totalPremium = (basePremium + optionalCoverages) * stateAdjustment + surcharges;
        var minimumPremium = _premiumCalculator.GetStateMinimumPremium(quote.StateCode);
        totalPremium = Math.Max(totalPremium, minimumPremium);

        quote.BasePremium = Math.Round(basePremium, 2);
        quote.TotalPremium = Math.Round(totalPremium, 2);
        quote.PremiumCalculationDetails = $"Base Rate: ${baseRate:F2}\n" +
            $"Property Value Factor: {propertyValueFactor}\nConstruction Factor: {constructionFactor}\n" +
            $"Age Factor: {ageFactor}\nOccupancy Factor: {occupancyFactor}\n" +
            $"Protection Factor: {protectionFactor}\nTerritory Factor: {territoryFactor}\n" +
            $"Catastrophe Factor: {catFactor}\nRoof Factor: {roofFactor}\n" +
            $"Sq Footage Factor: {sqftFactor}\nStories Factor: {storiesFactor}\n" +
            $"Loss History Factor: {lossFactor}\nDeductible Credit: {deductibleCredit}\n" +
            $"Optional Coverages: ${optionalCoverages:F2}\nState Adjustment: {stateAdjustment}\n" +
            $"Surcharges/Taxes: ${surcharges:F2}";
    }

    private static decimal GetBaseRate(string stateCode, string constructionType)
    {
        var stateRate = stateCode switch
        {
            "CA" => 850m, "TX" => 720m, "FL" => 980m, "NY" => 890m,
            "IL" => 740m, "PA" => 710m, "OH" => 680m, "GA" => 700m,
            "NC" => 690m, "MI" => 730m, "NJ" => 870m, "VA" => 695m,
            "WA" => 750m, "AZ" => 780m, "MA" => 860m, "TN" => 670m,
            "IN" => 665m, "MO" => 675m, "MD" => 820m, "WI" => 720m,
            _ => 700m
        };

        var constructionMultiplier = constructionType switch
        {
            "Frame" => 1.25m,
            "Joisted Masonry" => 1.10m,
            "Non-Combustible" => 0.95m,
            "Masonry Non-Combustible" => 0.85m,
            "Modified Fire Resistive" => 0.75m,
            "Fire Resistive" => 0.65m,
            _ => 1.00m
        };

        return stateRate * constructionMultiplier;
    }

    private static string GenerateQuoteNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return $"Q{datePart}-{uniquePart}";
    }
}

public class ValidationException : Exception
{
    public List<string> Errors { get; }
    public ValidationException(List<string> errors) : base(string.Join("; ", errors))
        => Errors = errors;
}

public class ComplianceException : Exception
{
    public List<string> Violations { get; }
    public ComplianceException(List<string> violations) : base(string.Join("; ", violations))
        => Violations = violations;
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
