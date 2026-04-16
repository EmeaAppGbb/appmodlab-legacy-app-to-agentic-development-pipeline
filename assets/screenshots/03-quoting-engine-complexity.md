# Screenshot: QuotingEngine.cs — The Heart of Legacy Complexity

The `QuotingEngine.cs` (685 lines) is the largest and most complex file,
implementing 40+ insurance rating factors across all 50 US states.

## Premium Calculation Pipeline (15 Steps)

```csharp
public PremiumCalculationResult CalculatePremium(Quote quote)
{
    // Step 1:  Base Rate Calculation         → GetBaseRate()
    // Step 2:  Property Value Factor         → CalculatePropertyValueFactor()
    // Step 3:  Construction Type Factor      → GetConstructionTypeFactor()
    // Step 4:  Age of Building Factor        → CalculateAgeFactor()
    // Step 5:  Occupancy Classification      → GetOccupancyFactor()
    // Step 6:  Protection Class Factor       → CalculateProtectionFactor()
    // Step 7:  Territory/Location Factor     → GetTerritoryFactor()
    // Step 8:  Catastrophe Zone Loading      → CalculateCatastropheLoading()
    // Step 9:  Roof Factor                   → CalculateRoofFactor()
    // Step 10: Square Footage Factor         → CalculateSquareFootageFactor()
    // Step 11: Stories Factor                → CalculateStoriesFactor()
    // Step 12: Loss History Factor           → CalculateLossHistoryFactor()
    // Step 13: Deductible Credit             → CalculateDeductibleCredit()
    // Step 14: State-Specific Adjustments    → ApplyStateSpecificAdjustments()
    // Step 15: Minimum Premium Enforcement   → GetMinimumPremium()
}
```

## State-Specific Complexity Example

```csharp
// California: Prop 103, earthquake, wildfire zones
case "CA":
    if (zipCode.StartsWith("900") || zipCode.StartsWith("941"))
        stateFactor *= 1.15m; // High-value urban
    premium += GetEarthquakeExposureCharge(quote);
    premium += GetWildfireZoneCharge(quote);
    premium *= 1.035m; // CA regulatory assessment

// Florida: Hurricane, wind mitigation, coastal rules
case "FL":
    premium += GetHurricaneCharge(propertyValue, constructionType);
    premium += GetFloodZoneCharge(stateCode, zipCode, propertyValue);
    if (roofAge > 15)
        premium *= 1.25m; // Old roof surcharge
    premium *= 1.0175m; // FL Hurricane Cat Fund
```

## Business Logic Depth

| Category                  | Factors | Lines |
|---------------------------|---------|-------|
| Base rating               | 6       | ~80   |
| Property characteristics  | 8       | ~120  |
| Location/catastrophe      | 10      | ~150  |
| Protection & safety       | 4       | ~60   |
| State-specific compliance | 50      | ~200  |
| Premium adjustments       | 5       | ~75   |
| **Total**                 | **40+** | **685**|

This is the exact type of deep, rule-heavy legacy code that makes
manual modernization risky and time-consuming — ideal for Spec2Cloud analysis.
