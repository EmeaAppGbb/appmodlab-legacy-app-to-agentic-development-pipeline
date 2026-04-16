using KeystoneInsurance.Modern.Domain.Entities;
using KeystoneInsurance.Modern.Domain.Rules;

namespace KeystoneInsurance.Modern.Services;

public record ComplianceResult(bool IsCompliant, List<string> Violations, List<string> Warnings);

public interface IComplianceService
{
    ComplianceResult ValidateCompliance(Quote quote);
}

public class ComplianceService : IComplianceService
{
    public ComplianceResult ValidateCompliance(Quote quote)
    {
        var violations = new List<string>();
        var warnings = new List<string>();

        ValidateGeneralCompliance(quote, violations);

        switch (quote.StateCode)
        {
            case "CA": ValidateCaliforniaCompliance(quote, violations, warnings); break;
            case "FL": ValidateFloridaCompliance(quote, violations, warnings); break;
            case "TX": ValidateTexasCompliance(quote, violations, warnings); break;
            case "NY": ValidateNewYorkCompliance(quote, violations, warnings); break;
            case "LA": ValidateLouisianaCompliance(quote, violations, warnings); break;
        }

        if (ComplianceRules.RequiresFloodInsuranceDisclosure(quote.StateCode))
            warnings.Add("Flood insurance disclosure required for this state");
        if (ComplianceRules.RequiresEarthquakeInsuranceOffer(quote.StateCode))
            warnings.Add("Earthquake insurance must be offered in this state");

        return new ComplianceResult(violations.Count == 0, violations, warnings);
    }

    private static void ValidateGeneralCompliance(Quote quote, List<string> violations)
    {
        if (quote.CoverageLimit < 100_000)
            violations.Add("Coverage limit below $100,000 minimum");
        if (quote.PropertyValue > 0 && quote.Deductible > quote.PropertyValue * 0.10m)
            violations.Add("Deductible exceeds 10% of property value");
    }

    private static void ValidateCaliforniaCompliance(Quote quote, List<string> violations, List<string> warnings)
    {
        if (quote.TotalPremium.HasValue && quote.TotalPremium < 500)
            violations.Add("California: Premium below $500 (Proposition 103 non-compliant)");
        if (quote.ZipCode.Length >= 2 && int.TryParse(quote.ZipCode[..2], out var prefix) && prefix >= 90 && prefix <= 95)
            warnings.Add("California: Earthquake coverage should be offered for this ZIP code");
        if (quote.PropertyValue > 3_000_000)
            warnings.Add("California: Property may require FAIR Plan excess coverage");
    }

    private static void ValidateFloridaCompliance(Quote quote, List<string> violations, List<string> warnings)
    {
        var buildingAge = DateTime.UtcNow.Year - quote.YearBuilt;
        if (buildingAge > 30 && quote.RoofAge > 15)
            violations.Add("Florida: Building age > 30 with roof age > 15 is non-compliant");

        var isCoastal = quote.ZipCode.Length >= 2 && quote.ZipCode[..2] is "32" or "33" or "34";
        if (isCoastal)
        {
            warnings.Add("Florida: Wind mitigation inspection required for coastal properties");
            if (quote.PropertyValue > 500_000 && !quote.SprinklersInstalled)
                violations.Add("Florida: Coastal property > $500K without sprinklers is non-compliant");
            if (quote.CoverageLimit > 700_000)
                warnings.Add("Florida: Coastal coverage > $700K may require Citizens participation");
        }
    }

    private static void ValidateTexasCompliance(Quote quote, List<string> violations, List<string> warnings)
    {
        if (quote.RoofType == "Asphalt Shingle" && quote.YearBuilt > 2015)
            warnings.Add("Texas: Class 4 impact-resistant shingles discount may apply");
        if (quote.PropertyValue > 0 && quote.Deductible < quote.PropertyValue * 0.01m)
            violations.Add("Texas: Minimum 1% wind/hail deductible required");
    }

    private static void ValidateNewYorkCompliance(Quote quote, List<string> violations, List<string> warnings)
    {
        if (quote.PropertyValue > 1_000_000 && string.IsNullOrWhiteSpace(quote.PropertyAddress))
            violations.Add("New York: Property address required for policies > $1M");
        if (quote.CoverageLimit < quote.PropertyValue * 0.80m)
            violations.Add("New York: Coverage must be at least 80% of replacement cost");
    }

    private static void ValidateLouisianaCompliance(Quote quote, List<string> violations, List<string> warnings)
    {
        var isCoastal = quote.ZipCode.Length >= 2 && quote.ZipCode[..2] is "70" or "71";
        if (isCoastal)
        {
            warnings.Add("Louisiana: Coastal parish - Citizens participation may apply");
            if (quote.PropertyValue > 0 && quote.Deductible < quote.PropertyValue * 0.02m)
                violations.Add("Louisiana: Coastal property requires minimum 2% deductible");
        }
    }
}
