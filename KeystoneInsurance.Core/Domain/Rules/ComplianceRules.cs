using System;
using System.Collections.Generic;

namespace KeystoneInsurance.Core.Domain.Rules
{
    public class ComplianceRules
    {
        public static Dictionary<string, decimal> GetStateMinimumPremiums()
        {
            return new Dictionary<string, decimal>
            {
                {"CA", 750.00m},
                {"FL", 800.00m},
                {"NY", 725.00m},
                {"TX", 650.00m},
                {"LA", 700.00m},
                {"NJ", 700.00m},
                {"MA", 675.00m}
            };
        }
        
        public static Dictionary<string, decimal> GetStateTaxRates()
        {
            return new Dictionary<string, decimal>
            {
                {"FL", 0.0175m},
                {"TX", 0.0185m},
                {"CA", 0.0230m},
                {"NY", 0.0300m},
                {"LA", 0.0475m}
            };
        }
        
        public static bool RequiresFloodInsuranceDisclosure(string stateCode, string zipCode)
        {
            // NFIP requires flood insurance disclosure in special flood hazard areas
            var floodProneStates = new[] { "FL", "LA", "TX", "NC", "SC", "GA", "AL", "MS" };
            return Array.Exists(floodProneStates, s => s == stateCode);
        }
        
        public static bool RequiresEarthquakeInsuranceOffer(string stateCode)
        {
            // Some states require earthquake insurance to be offered
            var earthquakeStates = new[] { "CA", "WA", "OR", "AK" };
            return Array.Exists(earthquakeStates, s => s == stateCode);
        }
        
        public static int GetMaximumCancellationNoticeDays(string stateCode)
        {
            // State-mandated notice periods for cancellation
            switch (stateCode)
            {
                case "CA":
                    return 75; // 75 days in California
                case "FL":
                    return 45; // 45 days in Florida
                case "NY":
                    return 60; // 60 days in New York
                default:
                    return 30; // 30 days default
            }
        }
    }
}
