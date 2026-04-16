namespace KeystoneInsurance.Modern.Domain.Rules;

/// <summary>
/// Static underwriting rules per spec UW-080 through UW-093.
/// </summary>
public static class UnderwritingRules
{
    public static bool IsAutoDecline(
        int buildingAge, string constructionType, int priorClaimsCount,
        decimal catastrophePml, decimal propertyValue)
    {
        // UW-080: Building age > 75 AND Frame AND ≥ 3 prior claims
        if (buildingAge > 75 && constructionType == "Frame" && priorClaimsCount >= 3)
            return true;

        // UW-081: Catastrophe PML > 60% of property value
        if (propertyValue > 0 && catastrophePml > propertyValue * 0.60m)
            return true;

        // UW-082: More than 5 prior claims
        if (priorClaimsCount > 5)
            return true;

        return false;
    }

    public static bool RequiresSeniorUnderwriterReview(
        decimal propertyValue, int buildingAge, string occupancyType, decimal riskScore)
    {
        // UW-085 through UW-088
        return propertyValue > 5_000_000m
            || buildingAge > 100
            || occupancyType is "Manufacturing-Heavy" or "Restaurant"
            || riskScore > 70;
    }

    public static bool RequiresPropertyInspection(
        int buildingAge, int roofAge, decimal propertyValue, int priorClaimsCount)
    {
        // UW-090 through UW-093
        return buildingAge > 30
            || roofAge > 15
            || propertyValue > 3_000_000m
            || priorClaimsCount >= 2;
    }
}
