using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KeystoneInsurance.Core.Domain.Entities;
using KeystoneInsurance.Core.Domain.Rules;

namespace KeystoneInsurance.Core.Services
{
    /// <summary>
    /// Complex quoting engine with 15 years of accumulated business rules
    /// This service calculates insurance premiums using 40+ rating factors across 50 states
    /// </summary>
    public class QuotingEngine
    {
        private readonly PremiumCalculator _premiumCalculator;
        private readonly ComplianceService _complianceService;
        
        public QuotingEngine()
        {
            _premiumCalculator = new PremiumCalculator();
            _complianceService = new ComplianceService();
        }
        
        public Quote GenerateQuote(Quote quote)
        {
            quote.QuoteNumber = GenerateQuoteNumber();
            quote.CreatedDate = DateTime.Now;
            quote.ExpirationDate = DateTime.Now.AddDays(30);
            quote.Status = "Draft";
            
            // Validate state compliance
            var complianceCheck = _complianceService.ValidateStateCompliance(quote.StateCode, quote);
            if (!complianceCheck.IsCompliant)
            {
                throw new InvalidOperationException($"Quote does not meet state compliance: {complianceCheck.Reason}");
            }
            
            // Calculate premium with complex rating algorithm
            var premiumResult = CalculatePremium(quote);
            quote.BasePremium = premiumResult.BasePremium;
            quote.TotalPremium = premiumResult.TotalPremium;
            quote.PremiumCalculationDetails = premiumResult.CalculationDetails;
            
            return quote;
        }
        
        public PremiumCalculationResult CalculatePremium(Quote quote)
        {
            var result = new PremiumCalculationResult();
            var details = new StringBuilder();
            
            // Step 1: Base Rate Calculation
            decimal baseRate = GetBaseRate(quote.StateCode, quote.ConstructionType, quote.OccupancyType);
            details.AppendLine($"Base Rate: ${baseRate:F2}");
            
            // Step 2: Property Value Factor
            decimal propertyValueFactor = CalculatePropertyValueFactor(quote.PropertyValue);
            details.AppendLine($"Property Value Factor: {propertyValueFactor:F4}");
            
            // Step 3: Construction Type Factor
            decimal constructionFactor = GetConstructionTypeFactor(quote.ConstructionType);
            details.AppendLine($"Construction Type Factor ({quote.ConstructionType}): {constructionFactor:F4}");
            
            // Step 4: Age of Building Factor
            int buildingAge = DateTime.Now.Year - quote.YearBuilt;
            decimal ageFactor = CalculateAgeFactor(buildingAge);
            details.AppendLine($"Building Age Factor ({buildingAge} years): {ageFactor:F4}");
            
            // Step 5: Occupancy Classification Factor
            decimal occupancyFactor = GetOccupancyFactor(quote.OccupancyType, quote.StateCode);
            details.AppendLine($"Occupancy Factor ({quote.OccupancyType}): {occupancyFactor:F4}");
            
            // Step 6: Protection Class Factor
            decimal protectionFactor = CalculateProtectionFactor(quote);
            details.AppendLine($"Protection Factor: {protectionFactor:F4}");
            
            // Step 7: Territory/Location Factor
            decimal territoryFactor = GetTerritoryFactor(quote.StateCode, quote.ZipCode);
            details.AppendLine($"Territory Factor ({quote.StateCode}-{quote.ZipCode}): {territoryFactor:F4}");
            
            // Step 8: Catastrophe Zone Factor
            decimal catFactor = CalculateCatastropheZoneFactor(quote.StateCode, quote.ZipCode);
            details.AppendLine($"Catastrophe Zone Factor: {catFactor:F4}");
            
            // Step 9: Roof Characteristics Factor
            decimal roofFactor = CalculateRoofFactor(quote.RoofType, quote.RoofAge);
            details.AppendLine($"Roof Factor ({quote.RoofType}, {quote.RoofAge} years old): {roofFactor:F4}");
            
            // Step 10: Square Footage Factor
            decimal sqFootageFactor = CalculateSquareFootageFactor(quote.SquareFootage);
            details.AppendLine($"Square Footage Factor ({quote.SquareFootage} sq ft): {sqFootageFactor:F4}");
            
            // Step 11: Number of Stories Factor
            decimal storiesFactor = GetStoriesFactor(quote.NumberOfStories);
            details.AppendLine($"Stories Factor ({quote.NumberOfStories} stories): {storiesFactor:F4}");
            
            // Step 12: Loss History Factor
            decimal lossHistoryFactor = CalculateLossHistoryFactor(quote.PriorClaimsCount, quote.PriorClaimsTotalAmount);
            details.AppendLine($"Loss History Factor: {lossHistoryFactor:F4}");
            
            // Step 13: Deductible Credit
            decimal deductibleCredit = CalculateDeductibleCredit(quote.Deductible, quote.PropertyValue);
            details.AppendLine($"Deductible Credit (${quote.Deductible}): {deductibleCredit:F4}");
            
            // Calculate base premium
            decimal basePremium = baseRate * propertyValueFactor * constructionFactor * ageFactor * 
                                 occupancyFactor * protectionFactor * territoryFactor * catFactor *
                                 roofFactor * sqFootageFactor * storiesFactor * lossHistoryFactor;
            
            basePremium = basePremium * deductibleCredit;
            
            result.BasePremium = Math.Round(basePremium, 2);
            details.AppendLine($"\nBase Premium: ${result.BasePremium:F2}");
            
            // Step 14: Optional Coverages
            decimal optionalCoveragesPremium = 0;
            
            if (quote.BusinessInterruptionCoverage)
            {
                decimal biPremium = CalculateBusinessInterruptionPremium(quote.BusinessInterruptionLimit, quote.OccupancyType);
                optionalCoveragesPremium += biPremium;
                details.AppendLine($"Business Interruption Coverage: ${biPremium:F2}");
            }
            
            if (quote.EquipmentBreakdownCoverage)
            {
                decimal ebPremium = CalculateEquipmentBreakdownPremium(quote.PropertyValue);
                optionalCoveragesPremium += ebPremium;
                details.AppendLine($"Equipment Breakdown Coverage: ${ebPremium:F2}");
            }
            
            if (quote.FloodCoverage)
            {
                decimal floodPremium = CalculateFloodPremium(quote.StateCode, quote.ZipCode, quote.CoverageLimit);
                optionalCoveragesPremium += floodPremium;
                details.AppendLine($"Flood Coverage: ${floodPremium:F2}");
            }
            
            if (quote.EarthquakeCoverage)
            {
                decimal eqPremium = CalculateEarthquakePremium(quote.StateCode, quote.ZipCode, quote.CoverageLimit);
                optionalCoveragesPremium += eqPremium;
                details.AppendLine($"Earthquake Coverage: ${eqPremium:F2}");
            }
            
            // Step 15: State-Specific Adjustments
            decimal stateAdjustmentFactor = GetStateSpecificAdjustmentFactor(quote.StateCode);
            details.AppendLine($"\nState Adjustment Factor ({quote.StateCode}): {stateAdjustmentFactor:F4}");
            
            // Step 16: Apply state-specific minimum premiums
            decimal stateMinimum = GetStateMinimumPremium(quote.StateCode);
            
            // Calculate total
            decimal totalPremium = (result.BasePremium + optionalCoveragesPremium) * stateAdjustmentFactor;
            
            // Apply state minimum
            if (totalPremium < stateMinimum)
            {
                details.AppendLine($"\nApplying state minimum premium: ${stateMinimum:F2}");
                totalPremium = stateMinimum;
            }
            
            // Step 17: State-Specific Surcharges and Taxes
            decimal surchargesAndTaxes = CalculateSurchargesAndTaxes(totalPremium, quote.StateCode);
            details.AppendLine($"Surcharges and Taxes: ${surchargesAndTaxes:F2}");
            totalPremium += surchargesAndTaxes;
            
            result.TotalPremium = Math.Round(totalPremium, 2);
            details.AppendLine($"\n=== TOTAL PREMIUM: ${result.TotalPremium:F2} ===");
            
            result.CalculationDetails = details.ToString();
            return result;
        }
        
        private decimal GetBaseRate(string stateCode, string constructionType, string occupancyType)
        {
            // Simplified base rate lookup - in reality this would query database
            var baseRates = new Dictionary<string, decimal>
            {
                {"CA", 850.00m}, {"TX", 720.00m}, {"FL", 980.00m}, {"NY", 890.00m},
                {"IL", 740.00m}, {"PA", 710.00m}, {"OH", 680.00m}, {"GA", 700.00m},
                {"NC", 690.00m}, {"MI", 730.00m}, {"NJ", 870.00m}, {"VA", 695.00m},
                {"WA", 750.00m}, {"AZ", 780.00m}, {"MA", 860.00m}, {"TN", 670.00m},
                {"IN", 665.00m}, {"MO", 675.00m}, {"MD", 820.00m}, {"WI", 720.00m}
            };
            
            decimal baseRate = baseRates.ContainsKey(stateCode) ? baseRates[stateCode] : 700.00m;
            
            // Adjust for construction type
            switch (constructionType)
            {
                case "Frame":
                    baseRate *= 1.25m;
                    break;
                case "Joisted Masonry":
                    baseRate *= 1.10m;
                    break;
                case "Non-Combustible":
                    baseRate *= 0.95m;
                    break;
                case "Masonry Non-Combustible":
                    baseRate *= 0.85m;
                    break;
                case "Modified Fire Resistive":
                    baseRate *= 0.75m;
                    break;
                case "Fire Resistive":
                    baseRate *= 0.65m;
                    break;
            }
            
            return baseRate;
        }
        
        private decimal CalculatePropertyValueFactor(decimal propertyValue)
        {
            // Progressive rating based on property value tiers
            if (propertyValue < 100000m)
                return 0.0025m;
            else if (propertyValue < 250000m)
                return 0.0023m;
            else if (propertyValue < 500000m)
                return 0.0021m;
            else if (propertyValue < 1000000m)
                return 0.0019m;
            else if (propertyValue < 2500000m)
                return 0.0017m;
            else if (propertyValue < 5000000m)
                return 0.0015m;
            else
                return 0.0013m;
        }
        
        private decimal GetConstructionTypeFactor(string constructionType)
        {
            switch (constructionType)
            {
                case "Frame":
                    return 1.45m;
                case "Joisted Masonry":
                    return 1.25m;
                case "Non-Combustible":
                    return 1.10m;
                case "Masonry Non-Combustible":
                    return 0.95m;
                case "Modified Fire Resistive":
                    return 0.80m;
                case "Fire Resistive":
                    return 0.65m;
                default:
                    return 1.00m;
            }
        }
        
        private decimal CalculateAgeFactor(int buildingAge)
        {
            // Age surcharges based on building age
            if (buildingAge < 5)
                return 0.90m; // New construction credit
            else if (buildingAge < 10)
                return 0.95m;
            else if (buildingAge < 20)
                return 1.00m;
            else if (buildingAge < 30)
                return 1.05m;
            else if (buildingAge < 40)
                return 1.15m;
            else if (buildingAge < 50)
                return 1.25m;
            else if (buildingAge < 75)
                return 1.40m;
            else
                return 1.60m; // Historic building surcharge
        }
        
        private decimal GetOccupancyFactor(string occupancyType, string stateCode)
        {
            // Base occupancy factors
            var occupancyFactors = new Dictionary<string, decimal>
            {
                {"Office", 0.85m},
                {"Retail", 1.00m},
                {"Restaurant", 1.35m},
                {"Manufacturing-Light", 1.15m},
                {"Manufacturing-Heavy", 1.50m},
                {"Warehouse", 0.95m},
                {"Mixed-Use", 1.10m},
                {"Medical", 1.05m},
                {"Educational", 0.80m},
                {"Hotel", 1.25m},
                {"Apartment", 0.90m}
            };
            
            decimal factor = occupancyFactors.ContainsKey(occupancyType) ? occupancyFactors[occupancyType] : 1.00m;
            
            // State-specific occupancy adjustments
            if (stateCode == "CA" && (occupancyType == "Restaurant" || occupancyType == "Hotel"))
                factor *= 1.10m; // Higher liability in CA
            
            if (stateCode == "FL" && occupancyType == "Retail")
                factor *= 1.05m; // Hurricane exposure for retail
                
            return factor;
        }
        
        private decimal CalculateProtectionFactor(Quote quote)
        {
            decimal factor = 1.00m;
            
            // Sprinkler system credits
            if (quote.SprinklersInstalled)
            {
                factor *= 0.75m; // 25% credit for sprinklers
            }
            
            // Alarm system credits
            if (quote.AlarmSystemInstalled)
            {
                factor *= 0.90m; // 10% credit for alarm
            }
            
            // Combined systems bonus
            if (quote.SprinklersInstalled && quote.AlarmSystemInstalled)
            {
                factor *= 0.95m; // Additional 5% for both
            }
            
            return factor;
        }
        
        private decimal GetTerritoryFactor(string stateCode, string zipCode)
        {
            // Territory factors vary by state and ZIP code
            // This is simplified - real implementation would use extensive territory tables
            
            // High-risk ZIP codes (coastal, urban, etc.)
            var highRiskZipPrefixes = new[] { "90", "33", "07", "10", "11", "77", "94" };
            
            if (highRiskZipPrefixes.Any(prefix => zipCode.StartsWith(prefix)))
            {
                return 1.25m;
            }
            
            // Medium-risk territories
            var mediumRiskStates = new[] { "CA", "FL", "TX", "NY", "NJ" };
            if (mediumRiskStates.Contains(stateCode))
            {
                return 1.10m;
            }
            
            return 1.00m;
        }
        
        private decimal CalculateCatastropheZoneFactor(string stateCode, string zipCode)
        {
            decimal factor = 1.00m;
            
            // Hurricane zones
            var hurricaneStates = new[] { "FL", "LA", "MS", "AL", "TX", "NC", "SC", "GA" };
            if (hurricaneStates.Contains(stateCode))
            {
                factor *= 1.35m;
                
                // Coastal ZIP codes get additional surcharge
                var coastalZipPrefixes = new[] { "33", "34", "32", "70", "39", "77", "28", "29" };
                if (coastalZipPrefixes.Any(prefix => zipCode.StartsWith(prefix)))
                {
                    factor *= 1.20m;
                }
            }
            
            // Earthquake zones
            var earthquakeStates = new[] { "CA", "AK", "WA", "OR", "NV" };
            if (earthquakeStates.Contains(stateCode))
            {
                factor *= 1.25m;
            }
            
            // Tornado alley
            var tornadoStates = new[] { "OK", "KS", "NE", "TX", "SD" };
            if (tornadoStates.Contains(stateCode))
            {
                factor *= 1.15m;
            }
            
            return factor;
        }
        
        private decimal CalculateRoofFactor(string roofType, int roofAge)
        {
            decimal factor = 1.00m;
            
            // Roof type factors
            switch (roofType)
            {
                case "Asphalt Shingle":
                    factor = 1.10m;
                    break;
                case "Metal":
                    factor = 0.85m;
                    break;
                case "Tile":
                    factor = 0.90m;
                    break;
                case "Slate":
                    factor = 0.80m;
                    break;
                case "Flat/Built-Up":
                    factor = 1.15m;
                    break;
                case "TPO/EPDM":
                    factor = 0.95m;
                    break;
            }
            
            // Roof age surcharges
            if (roofAge > 20)
                factor *= 1.40m;
            else if (roofAge > 15)
                factor *= 1.25m;
            else if (roofAge > 10)
                factor *= 1.10m;
            else if (roofAge < 3)
                factor *= 0.95m; // New roof credit
                
            return factor;
        }
        
        private decimal CalculateSquareFootageFactor(int squareFootage)
        {
            // Economies of scale for larger buildings
            if (squareFootage < 5000)
                return 1.15m;
            else if (squareFootage < 10000)
                return 1.05m;
            else if (squareFootage < 25000)
                return 1.00m;
            else if (squareFootage < 50000)
                return 0.95m;
            else if (squareFootage < 100000)
                return 0.90m;
            else
                return 0.85m;
        }
        
        private decimal GetStoriesFactor(int numberOfStories)
        {
            if (numberOfStories == 1)
                return 0.95m;
            else if (numberOfStories == 2)
                return 1.00m;
            else if (numberOfStories <= 4)
                return 1.10m;
            else if (numberOfStories <= 6)
                return 1.20m;
            else if (numberOfStories <= 10)
                return 1.35m;
            else
                return 1.50m; // High-rise surcharge
        }
        
        private decimal CalculateLossHistoryFactor(int priorClaimsCount, decimal priorClaimsTotalAmount)
        {
            if (priorClaimsCount == 0)
                return 0.90m; // Loss-free credit
                
            decimal factor = 1.00m;
            
            // Claims frequency surcharge
            if (priorClaimsCount == 1)
                factor *= 1.15m;
            else if (priorClaimsCount == 2)
                factor *= 1.30m;
            else if (priorClaimsCount >= 3)
                factor *= 1.50m;
            
            // Claims severity surcharge
            if (priorClaimsTotalAmount > 500000m)
                factor *= 1.40m;
            else if (priorClaimsTotalAmount > 250000m)
                factor *= 1.25m;
            else if (priorClaimsTotalAmount > 100000m)
                factor *= 1.15m;
            else if (priorClaimsTotalAmount > 50000m)
                factor *= 1.10m;
                
            return factor;
        }
        
        private decimal CalculateDeductibleCredit(decimal deductible, decimal propertyValue)
        {
            decimal deductiblePercentage = deductible / propertyValue;
            
            if (deductiblePercentage >= 0.05m) // 5% or higher
                return 0.70m;
            else if (deductiblePercentage >= 0.03m) // 3%
                return 0.80m;
            else if (deductiblePercentage >= 0.02m) // 2%
                return 0.85m;
            else if (deductiblePercentage >= 0.01m) // 1%
                return 0.90m;
            else if (deductiblePercentage >= 0.005m) // 0.5%
                return 0.95m;
            else
                return 1.00m; // No credit
        }
        
        private decimal CalculateBusinessInterruptionPremium(decimal limit, string occupancyType)
        {
            // Base BI rate varies by occupancy
            decimal baseRate = 0.0035m;
            
            if (occupancyType == "Restaurant" || occupancyType == "Retail")
                baseRate = 0.0045m;
            else if (occupancyType == "Manufacturing-Heavy")
                baseRate = 0.0050m;
            else if (occupancyType == "Office")
                baseRate = 0.0025m;
                
            return limit * baseRate;
        }
        
        private decimal CalculateEquipmentBreakdownPremium(decimal propertyValue)
        {
            return propertyValue * 0.0008m;
        }
        
        private decimal CalculateFloodPremium(string stateCode, string zipCode, decimal coverageLimit)
        {
            // Flood zones require NFIP or private flood insurance
            // Rates vary significantly by flood zone designation
            
            var highFloodRiskStates = new[] { "FL", "LA", "TX", "NC", "SC" };
            decimal baseRate = 0.0025m;
            
            if (highFloodRiskStates.Contains(stateCode))
            {
                baseRate = 0.0055m;
            }
            
            return coverageLimit * baseRate;
        }
        
        private decimal CalculateEarthquakePremium(string stateCode, string zipCode, decimal coverageLimit)
        {
            // Earthquake premium heavily dependent on location
            var highEqRiskStates = new[] { "CA", "AK", "WA", "OR" };
            
            if (!highEqRiskStates.Contains(stateCode))
                return coverageLimit * 0.0010m; // Minimal risk
                
            // California has the highest rates
            if (stateCode == "CA")
                return coverageLimit * 0.0085m;
            else if (stateCode == "AK")
                return coverageLimit * 0.0070m;
            else
                return coverageLimit * 0.0045m;
        }
        
        private decimal GetStateSpecificAdjustmentFactor(string stateCode)
        {
            // State-specific regulatory and market adjustments
            var stateAdjustments = new Dictionary<string, decimal>
            {
                {"CA", 1.15m}, // High regulation and litigation costs
                {"FL", 1.20m}, // Hurricane exposure and AOB fraud
                {"NY", 1.12m}, // High regulatory requirements
                {"TX", 1.05m}, // Hail and wind exposure
                {"LA", 1.18m}, // Hurricane and flood exposure
                {"NJ", 1.10m}, // High cost of doing business
                {"IL", 1.03m},
                {"PA", 1.02m}
            };
            
            return stateAdjustments.ContainsKey(stateCode) ? stateAdjustments[stateCode] : 1.00m;
        }
        
        private decimal GetStateMinimumPremium(string stateCode)
        {
            // State-mandated minimum premiums
            var stateMinimums = new Dictionary<string, decimal>
            {
                {"CA", 750.00m},
                {"FL", 800.00m},
                {"NY", 725.00m},
                {"TX", 650.00m},
                {"LA", 700.00m}
            };
            
            return stateMinimums.ContainsKey(stateCode) ? stateMinimums[stateCode] : 500.00m;
        }
        
        private decimal CalculateSurchargesAndTaxes(decimal premium, string stateCode)
        {
            decimal total = 0;
            
            // State taxes
            var stateTaxRates = new Dictionary<string, decimal>
            {
                {"FL", 0.0175m}, // 1.75% state tax
                {"TX", 0.0185m}, // 1.85% state tax
                {"CA", 0.0230m}, // 2.30% state tax
                {"NY", 0.0300m}, // 3.00% state tax
                {"LA", 0.0475m}  // 4.75% state tax (highest)
            };
            
            if (stateTaxRates.ContainsKey(stateCode))
            {
                total += premium * stateTaxRates[stateCode];
            }
            
            // Catastrophe fund surcharges
            if (stateCode == "FL")
            {
                total += premium * 0.0200m; // Florida Hurricane Catastrophe Fund
            }
            
            // Fire marshal fees
            total += 25.00m;
            
            // Terrorism surcharge (TRIA)
            total += premium * 0.0015m;
            
            return Math.Round(total, 2);
        }
        
        private string GenerateQuoteNumber()
        {
            return $"Q{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
        
        public Quote RecalculateQuote(Quote existingQuote)
        {
            // Recalculate if quote details changed
            var premiumResult = CalculatePremium(existingQuote);
            existingQuote.BasePremium = premiumResult.BasePremium;
            existingQuote.TotalPremium = premiumResult.TotalPremium;
            existingQuote.PremiumCalculationDetails = premiumResult.CalculationDetails;
            
            return existingQuote;
        }
        
        public bool ValidateQuote(Quote quote)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(quote.StateCode))
                errors.Add("State code is required");
                
            if (quote.PropertyValue <= 0)
                errors.Add("Property value must be greater than zero");
                
            if (quote.CoverageLimit <= 0)
                errors.Add("Coverage limit must be greater than zero");
                
            if (quote.CoverageLimit > quote.PropertyValue * 1.5m)
                errors.Add("Coverage limit cannot exceed 150% of property value");
                
            if (quote.Deductible < 500)
                errors.Add("Minimum deductible is $500");
                
            if (string.IsNullOrEmpty(quote.ConstructionType))
                errors.Add("Construction type is required");
                
            if (string.IsNullOrEmpty(quote.OccupancyType))
                errors.Add("Occupancy type is required");
                
            if (quote.YearBuilt < 1800 || quote.YearBuilt > DateTime.Now.Year)
                errors.Add("Invalid year built");
                
            if (quote.SquareFootage <= 0)
                errors.Add("Square footage must be greater than zero");
                
            return errors.Count == 0;
        }
    }
    
    public class PremiumCalculationResult
    {
        public decimal BasePremium { get; set; }
        public decimal TotalPremium { get; set; }
        public string CalculationDetails { get; set; }
    }
}
