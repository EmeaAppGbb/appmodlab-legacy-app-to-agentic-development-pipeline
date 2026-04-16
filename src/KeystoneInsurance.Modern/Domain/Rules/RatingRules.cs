namespace KeystoneInsurance.Modern.Domain.Rules;

/// <summary>
/// Static rating eligibility rules per spec RR-001 through RR-006.
/// </summary>
public static class RatingRules
{
    public static bool IsEligibleForPreferredRate(
        int buildingAge, string constructionType, bool sprinklers, int priorClaimsCount)
    {
        var preferredConstructionTypes = new[] { "Fire Resistive", "Modified Fire Resistive" };
        return buildingAge < 20
            && preferredConstructionTypes.Contains(constructionType)
            && sprinklers
            && priorClaimsCount == 0;
    }

    public static bool RequiresWindMitigationInspection(string stateCode)
    {
        return stateCode is "FL" or "LA" or "TX" or "NC" or "SC";
    }

    public static bool IsHighValueProperty(decimal propertyValue)
    {
        return propertyValue > 2_000_000m;
    }

    public static decimal GetMinimumDeductible(string stateCode, decimal propertyValue)
    {
        return stateCode switch
        {
            "FL" when propertyValue > 500_000m => propertyValue * 0.02m,
            "TX" => propertyValue * 0.01m,
            _ => 1_000m
        };
    }
}
