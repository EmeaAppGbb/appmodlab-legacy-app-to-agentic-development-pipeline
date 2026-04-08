using System;
using KeystoneInsurance.Core.Domain.Entities;

namespace KeystoneInsurance.Core.Services
{
    public class RenewalService
    {
        private readonly QuotingEngine _quotingEngine;
        private readonly PolicyService _policyService;
        
        public RenewalService()
        {
            _quotingEngine = new QuotingEngine();
            _policyService = new PolicyService();
        }
        
        public Quote GenerateRenewalQuote(Policy expiringPolicy, Quote originalQuote)
        {
            // Create renewal quote based on expiring policy
            var renewalQuote = new Quote
            {
                ClientId = originalQuote.ClientId,
                QuoteNumber = $"RNW-{expiringPolicy.PolicyNumber}",
                CreatedDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddDays(30),
                Status = "Draft",
                
                // Copy property information from original quote
                PropertyAddress = originalQuote.PropertyAddress,
                City = originalQuote.City,
                StateCode = originalQuote.StateCode,
                ZipCode = originalQuote.ZipCode,
                PropertyValue = originalQuote.PropertyValue,
                ConstructionType = originalQuote.ConstructionType,
                OccupancyType = originalQuote.OccupancyType,
                YearBuilt = originalQuote.YearBuilt,
                SquareFootage = originalQuote.SquareFootage,
                NumberOfStories = originalQuote.NumberOfStories,
                SprinklersInstalled = originalQuote.SprinklersInstalled,
                AlarmSystemInstalled = originalQuote.AlarmSystemInstalled,
                RoofType = originalQuote.RoofType,
                RoofAge = originalQuote.RoofAge + 1, // Age the roof by one year
                
                // Copy coverage from expiring policy
                CoverageLimit = expiringPolicy.CoverageLimit,
                Deductible = expiringPolicy.Deductible,
                BusinessInterruptionCoverage = expiringPolicy.BusinessInterruptionCoverage,
                BusinessInterruptionLimit = expiringPolicy.BusinessInterruptionLimit,
                EquipmentBreakdownCoverage = expiringPolicy.EquipmentBreakdownCoverage,
                FloodCoverage = expiringPolicy.FloodCoverage,
                EarthquakeCoverage = expiringPolicy.EarthquakeCoverage,
                
                // Update claims history (would normally query claims system)
                PriorClaimsCount = originalQuote.PriorClaimsCount,
                PriorClaimsTotalAmount = originalQuote.PriorClaimsTotalAmount
            };
            
            // Apply renewal rate changes
            renewalQuote = ApplyRenewalFactors(renewalQuote, expiringPolicy);
            
            // Recalculate premium
            renewalQuote = _quotingEngine.GenerateQuote(renewalQuote);
            
            return renewalQuote;
        }
        
        private Quote ApplyRenewalFactors(Quote renewalQuote, Policy expiringPolicy)
        {
            // Apply trend factor (rate increases/decreases)
            decimal trendFactor = GetTrendFactor(renewalQuote.StateCode);
            
            // Apply loss experience factor
            decimal lossExperienceFactor = CalculateLossExperienceFactor(expiringPolicy);
            
            // Inflation adjustment for property value
            renewalQuote.PropertyValue = renewalQuote.PropertyValue * 1.03m; // 3% inflation
            renewalQuote.CoverageLimit = renewalQuote.CoverageLimit * 1.03m;
            
            return renewalQuote;
        }
        
        private decimal GetTrendFactor(string stateCode)
        {
            // State-specific trend factors (would come from rate filing system)
            switch (stateCode)
            {
                case "FL":
                    return 1.08m; // 8% increase due to hurricane activity
                case "CA":
                    return 1.12m; // 12% increase due to wildfire risk
                case "TX":
                    return 1.05m; // 5% increase due to hail losses
                case "LA":
                    return 1.10m; // 10% increase due to hurricane exposure
                default:
                    return 1.03m; // 3% general trend
            }
        }
        
        private decimal CalculateLossExperienceFactor(Policy policy)
        {
            // Adjust based on policy claims experience
            // This would query the claims system in a real implementation
            return 1.00m; // No claims = no adjustment
        }
    }
}
