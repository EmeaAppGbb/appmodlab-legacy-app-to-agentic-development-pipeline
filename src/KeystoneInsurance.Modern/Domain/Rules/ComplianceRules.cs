namespace KeystoneInsurance.Modern.Domain.Rules;

/// <summary>
/// Static compliance rules per spec CS-060 through CS-062.
/// </summary>
public static class ComplianceRules
{
    private static readonly HashSet<string> FloodDisclosureStates =
        ["FL", "LA", "TX", "NC", "SC", "GA", "AL", "MS"];

    private static readonly HashSet<string> EarthquakeOfferStates =
        ["CA", "WA", "OR", "AK"];

    public static bool RequiresFloodInsuranceDisclosure(string stateCode)
        => FloodDisclosureStates.Contains(stateCode);

    public static bool RequiresEarthquakeInsuranceOffer(string stateCode)
        => EarthquakeOfferStates.Contains(stateCode);

    public static int GetMaximumCancellationNoticeDays(string stateCode)
    {
        return stateCode switch
        {
            "CA" => 75,
            "NY" => 60,
            "FL" => 45,
            _ => 30
        };
    }
}
