using System;

namespace KeystoneInsurance.Core.Domain.Rules
{
    public class RatingRules
    {
        public static bool IsEligibleForPreferredRate(int buildingAge, string constructionType, 
            bool hasSprinklers, int priorClaims)
        {
            return buildingAge < 20 && 
                   (constructionType == "Fire Resistive" || constructionType == "Modified Fire Resistive") &&
                   hasSprinklers &&
                   priorClaims == 0;
        }
        
        public static bool RequiresWindMitigationInspection(string stateCode, string zipCode)
        {
            var coastalStates = new[] { "FL", "LA", "TX", "NC", "SC" };
            return Array.Exists(coastalStates, s => s == stateCode);
        }
        
        public static bool IsHighValueProperty(decimal propertyValue)
        {
            return propertyValue > 2000000m;
        }
        
        public static decimal GetMinimumDeductible(string stateCode, decimal propertyValue)
        {
            if (stateCode == "FL" && propertyValue > 500000)
                return propertyValue * 0.02m; // 2% minimum for high-value FL properties
            if (stateCode == "TX")
                return propertyValue * 0.01m; // 1% minimum in TX
            return 1000m; // Default minimum
        }
    }
}
