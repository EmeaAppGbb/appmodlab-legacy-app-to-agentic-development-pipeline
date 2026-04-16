using KeystoneInsurance.Modern.Domain.Entities;

namespace KeystoneInsurance.Modern.Services;

public interface IPremiumCalculator
{
    decimal GetPropertyValueFactor(decimal propertyValue);
    decimal GetConstructionTypeFactor(string constructionType);
    decimal GetBuildingAgeFactor(int buildingAge);
    decimal GetOccupancyFactor(string occupancyType, string stateCode);
    decimal GetProtectionFactor(bool sprinklers, bool alarm);
    decimal GetTerritoryFactor(string zipCode, string stateCode);
    decimal GetCatastropheZoneFactor(string stateCode, string zipCode);
    decimal GetRoofFactor(string? roofType, int roofAge);
    decimal GetSquareFootageFactor(int squareFootage);
    decimal GetStoriesFactor(int stories);
    decimal GetLossHistoryFactor(int priorClaimsCount, decimal priorClaimsTotalAmount);
    decimal GetDeductibleCredit(decimal deductible, decimal propertyValue);
    decimal CalculateOptionalCoverages(Quote quote);
    decimal GetStateAdjustmentFactor(string stateCode);
    decimal CalculateSurchargesAndTaxes(string stateCode, decimal premium);
    decimal GetStateMinimumPremium(string stateCode);
    decimal CalculateProratedPremium(decimal annualPremium, int daysInTerm);
    decimal CalculateReturnPremium(decimal annualPremium, int daysRemaining, string cancellationType);
    decimal CalculateInstallmentAmount(decimal annualPremium, string paymentPlan);
}

public class PremiumCalculator : IPremiumCalculator
{
    public decimal GetPropertyValueFactor(decimal propertyValue) => propertyValue switch
    {
        < 100_000m => 0.0025m,
        < 250_000m => 0.0023m,
        < 500_000m => 0.0021m,
        < 1_000_000m => 0.0019m,
        < 2_500_000m => 0.0017m,
        < 5_000_000m => 0.0015m,
        _ => 0.0013m
    };

    public decimal GetConstructionTypeFactor(string constructionType) => constructionType switch
    {
        "Frame" => 1.45m,
        "Joisted Masonry" => 1.25m,
        "Non-Combustible" => 1.10m,
        "Masonry Non-Combustible" => 0.95m,
        "Modified Fire Resistive" => 0.80m,
        "Fire Resistive" => 0.65m,
        _ => 1.00m
    };

    public decimal GetBuildingAgeFactor(int buildingAge) => buildingAge switch
    {
        < 5 => 0.90m,
        < 10 => 0.95m,
        < 20 => 1.00m,
        < 30 => 1.05m,
        < 40 => 1.15m,
        < 50 => 1.25m,
        < 75 => 1.40m,
        _ => 1.60m
    };

    public decimal GetOccupancyFactor(string occupancyType, string stateCode)
    {
        var baseFactor = occupancyType switch
        {
            "Office" => 0.85m,
            "Retail" => 1.00m,
            "Restaurant" => 1.35m,
            "Manufacturing-Light" => 1.15m,
            "Manufacturing-Heavy" => 1.50m,
            "Warehouse" => 0.95m,
            "Mixed-Use" => 1.10m,
            "Medical" => 1.05m,
            "Educational" => 0.80m,
            "Hotel" => 1.25m,
            "Apartment" => 0.90m,
            _ => 1.00m
        };

        // State-specific adjustments
        if (stateCode == "FL" && occupancyType == "Retail")
            baseFactor *= 1.05m;
        if (stateCode == "CA" && occupancyType is "Restaurant" or "Hotel")
            baseFactor *= 1.10m;

        return baseFactor;
    }

    public decimal GetProtectionFactor(bool sprinklers, bool alarm)
    {
        var factor = 1.00m;
        if (sprinklers) factor *= 0.75m;
        if (alarm) factor *= 0.90m;
        if (sprinklers && alarm) factor *= 0.95m; // combo bonus
        return factor;
    }

    public decimal GetTerritoryFactor(string zipCode, string stateCode)
    {
        var highRiskPrefixes = new[] { "90", "33", "07", "10", "11", "77", "94" };
        if (zipCode.Length >= 2 && highRiskPrefixes.Any(p => zipCode.StartsWith(p)))
            return 1.25m;

        var mediumRiskStates = new[] { "CA", "FL", "TX", "NY", "NJ" };
        if (mediumRiskStates.Contains(stateCode))
            return 1.10m;

        return 1.00m;
    }

    public decimal GetCatastropheZoneFactor(string stateCode, string zipCode)
    {
        var factor = 1.00m;

        var hurricaneStates = new[] { "FL", "LA", "MS", "AL", "TX", "NC", "SC", "GA" };
        if (hurricaneStates.Contains(stateCode))
        {
            factor *= 1.35m;
            var coastalPrefixes = new[] { "33", "34", "32", "70", "39", "77", "28", "29" };
            if (zipCode.Length >= 2 && coastalPrefixes.Any(p => zipCode.StartsWith(p)))
                factor *= 1.20m;
        }

        var earthquakeStates = new[] { "CA", "AK", "WA", "OR", "NV" };
        if (earthquakeStates.Contains(stateCode))
            factor *= 1.25m;

        var tornadoStates = new[] { "OK", "KS", "NE", "TX", "SD" };
        if (tornadoStates.Contains(stateCode))
            factor *= 1.15m;

        return factor;
    }

    public decimal GetRoofFactor(string? roofType, int roofAge)
    {
        var typeFactor = roofType switch
        {
            "Asphalt Shingle" => 1.10m,
            "Metal" => 0.85m,
            "Tile" => 0.90m,
            "Slate" => 0.80m,
            "Flat/Built-Up" => 1.15m,
            "TPO/EPDM" => 0.95m,
            _ => 1.00m
        };

        var ageFactor = roofAge switch
        {
            < 3 => 0.95m,
            <= 10 => 1.00m,
            <= 15 => 1.10m,
            <= 20 => 1.25m,
            _ => 1.40m
        };

        return typeFactor * ageFactor;
    }

    public decimal GetSquareFootageFactor(int squareFootage) => squareFootage switch
    {
        < 5_000 => 1.15m,
        < 10_000 => 1.05m,
        < 25_000 => 1.00m,
        < 50_000 => 0.95m,
        < 100_000 => 0.90m,
        _ => 0.85m
    };

    public decimal GetStoriesFactor(int stories) => stories switch
    {
        1 => 0.95m,
        2 => 1.00m,
        <= 4 => 1.10m,
        <= 6 => 1.20m,
        <= 10 => 1.35m,
        _ => 1.50m
    };

    public decimal GetLossHistoryFactor(int priorClaimsCount, decimal priorClaimsTotalAmount)
    {
        var claimsFactor = priorClaimsCount switch
        {
            0 => 0.90m,
            1 => 1.15m,
            2 => 1.30m,
            _ => 1.50m
        };

        var amountFactor = priorClaimsTotalAmount switch
        {
            > 500_000m => 1.40m,
            > 250_000m => 1.25m,
            > 100_000m => 1.15m,
            > 50_000m => 1.10m,
            _ => 1.00m
        };

        return claimsFactor * amountFactor;
    }

    public decimal GetDeductibleCredit(decimal deductible, decimal propertyValue)
    {
        if (propertyValue <= 0) return 1.00m;
        var pct = deductible / propertyValue;
        return pct switch
        {
            >= 0.05m => 0.70m,
            >= 0.03m => 0.80m,
            >= 0.02m => 0.85m,
            >= 0.01m => 0.90m,
            >= 0.005m => 0.95m,
            _ => 1.00m
        };
    }

    public decimal CalculateOptionalCoverages(Quote quote)
    {
        var total = 0m;

        if (quote.BusinessInterruptionCoverage && quote.BusinessInterruptionLimit.HasValue)
        {
            var biRate = quote.OccupancyType switch
            {
                "Office" => 0.0025m,
                "Restaurant" or "Retail" => 0.0045m,
                "Manufacturing-Heavy" => 0.0050m,
                _ => 0.0035m
            };
            total += quote.BusinessInterruptionLimit.Value * biRate;
        }

        if (quote.EquipmentBreakdownCoverage)
            total += quote.PropertyValue * 0.0008m;

        if (quote.FloodCoverage)
        {
            var floodRate = quote.StateCode is "FL" or "LA" or "TX" or "NC" or "SC" ? 0.0055m : 0.0025m;
            total += quote.CoverageLimit * floodRate;
        }

        if (quote.EarthquakeCoverage)
        {
            var eqRate = quote.StateCode switch
            {
                "CA" => 0.0085m,
                "AK" => 0.0070m,
                "WA" or "OR" => 0.0045m,
                _ => 0.0010m
            };
            total += quote.CoverageLimit * eqRate;
        }

        return total;
    }

    public decimal GetStateAdjustmentFactor(string stateCode) => stateCode switch
    {
        "CA" => 1.15m,
        "FL" => 1.20m,
        "NY" => 1.12m,
        "TX" => 1.05m,
        "LA" => 1.18m,
        "NJ" => 1.10m,
        "IL" => 1.03m,
        "PA" => 1.02m,
        _ => 1.00m
    };

    public decimal CalculateSurchargesAndTaxes(string stateCode, decimal premium)
    {
        var surcharges = 0m;

        // State taxes
        surcharges += stateCode switch
        {
            "FL" => premium * 0.0175m,
            "TX" => premium * 0.0185m,
            "CA" => premium * 0.0230m,
            "NY" => premium * 0.0300m,
            "LA" => premium * 0.0475m,
            _ => 0m
        };

        // FL Hurricane Cat Fund surcharge
        if (stateCode == "FL")
            surcharges += premium * 0.0200m;

        // Fire marshal fee (all states)
        surcharges += 25.00m;

        // TRIA terrorism surcharge (all states)
        surcharges += premium * 0.0015m;

        return Math.Round(surcharges, 2);
    }

    public decimal GetStateMinimumPremium(string stateCode) => stateCode switch
    {
        "CA" => 750m,
        "FL" => 800m,
        "NY" => 725m,
        "TX" => 650m,
        "LA" => 700m,
        _ => 500m
    };

    public decimal CalculateProratedPremium(decimal annualPremium, int daysInTerm)
        => Math.Round(annualPremium / 365m * daysInTerm, 2);

    public decimal CalculateReturnPremium(decimal annualPremium, int daysRemaining, string cancellationType)
    {
        return cancellationType switch
        {
            "ProRata" => Math.Round(annualPremium * daysRemaining / 365m, 2),
            "ShortRate" => Math.Round(annualPremium * daysRemaining / 365m * 0.90m, 2),
            "Flat" => 0m,
            _ => 0m
        };
    }

    public decimal CalculateInstallmentAmount(decimal annualPremium, string paymentPlan)
    {
        return paymentPlan switch
        {
            "Annual" => annualPremium,
            "SemiAnnual" => Math.Round(annualPremium / 2m * 1.03m, 2),
            "Quarterly" => Math.Round(annualPremium / 4m * 1.05m, 2),
            "Monthly" => Math.Round(annualPremium / 12m * 1.08m, 2),
            _ => annualPremium
        };
    }
}
