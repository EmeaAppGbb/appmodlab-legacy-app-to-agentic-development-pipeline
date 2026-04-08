using System;
using System.Collections.Generic;
using System.Linq;
using KeystoneInsurance.Core.Domain.Entities;

namespace KeystoneInsurance.Core.Services
{
    public class ComplianceService
    {
        public ComplianceCheckResult ValidateStateCompliance(string stateCode, Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // State-specific validations
            switch (stateCode)
            {
                case "CA":
                    result = ValidateCaliforniaCompliance(quote);
                    break;
                case "FL":
                    result = ValidateFloridaCompliance(quote);
                    break;
                case "TX":
                    result = ValidateTexasCompliance(quote);
                    break;
                case "NY":
                    result = ValidateNewYorkCompliance(quote);
                    break;
                case "LA":
                    result = ValidateLouisianaCompliance(quote);
                    break;
                default:
                    result = ValidateGeneralCompliance(quote);
                    break;
            }
            
            return result;
        }
        
        private ComplianceCheckResult ValidateCaliforniaCompliance(Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // California Proposition 103 requirements
            if (quote.TotalPremium < 500)
            {
                result.IsCompliant = false;
                result.Reason = "California: Premium below state minimum";
                return result;
            }
            
            // Earthquake coverage required in high-risk zones
            var highEqZips = new[] { "90", "91", "92", "93", "94", "95" };
            if (highEqZips.Any(zip => quote.ZipCode.StartsWith(zip)) && !quote.EarthquakeCoverage)
            {
                result.Warnings.Add("Earthquake coverage should be offered for properties in high-risk zones");
            }
            
            // Fair plan considerations for high-risk areas
            if (quote.PropertyValue > 3000000)
            {
                result.Warnings.Add("High-value property may require California FAIR Plan excess coverage");
            }
            
            return result;
        }
        
        private ComplianceCheckResult ValidateFloridaCompliance(Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // Florida Building Code requirements
            int buildingAge = DateTime.Now.Year - quote.YearBuilt;
            if (buildingAge > 30 && quote.RoofAge > 15)
            {
                result.IsCompliant = false;
                result.Reason = "Florida: Roof older than 15 years requires replacement for properties built before 1994";
                return result;
            }
            
            // Wind mitigation requirements
            var coastalZips = new[] { "32", "33", "34" };
            if (coastalZips.Any(zip => quote.ZipCode.StartsWith(zip)))
            {
                result.Warnings.Add("Coastal property requires wind mitigation inspection");
                
                if (!quote.SprinklersInstalled && quote.PropertyValue > 500000)
                {
                    result.IsCompliant = false;
                    result.Reason = "Florida: Coastal properties over $500k require sprinkler systems";
                    return result;
                }
            }
            
            // Citizens Property Insurance considerations
            if (quote.CoverageLimit > 700000 && coastalZips.Any(zip => quote.ZipCode.StartsWith(zip)))
            {
                result.Warnings.Add("May require Citizens Property Insurance participation");
            }
            
            return result;
        }
        
        private ComplianceCheckResult ValidateTexasCompliance(Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // Texas windstorm insurance requirements
            var coastalCounties = new[] { "Galveston", "Harris", "Nueces", "Cameron" };
            
            // Hail-resistant roofing requirements
            if (quote.RoofType == "Asphalt Shingle" && quote.YearBuilt > 2015)
            {
                result.Warnings.Add("Texas: Class 4 impact-resistant shingles may provide insurance discounts");
            }
            
            // Wind/hail deductible requirements
            if (quote.Deductible < quote.PropertyValue * 0.01m)
            {
                result.IsCompliant = false;
                result.Reason = "Texas: Minimum 1% wind/hail deductible required";
                return result;
            }
            
            return result;
        }
        
        private ComplianceCheckResult ValidateNewYorkCompliance(Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // New York insurance law requirements
            if (quote.PropertyValue > 1000000 && string.IsNullOrEmpty(quote.PropertyAddress))
            {
                result.IsCompliant = false;
                result.Reason = "New York: Complete property address required for properties over $1M";
                return result;
            }
            
            // Replacement cost requirements
            if (quote.CoverageLimit < quote.PropertyValue * 0.80m)
            {
                result.IsCompliant = false;
                result.Reason = "New York: Coverage must be at least 80% of replacement cost";
                return result;
            }
            
            return result;
        }
        
        private ComplianceCheckResult ValidateLouisianaCompliance(Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // Louisiana Citizens Property Insurance requirements
            var coastalParishes = new[] { "70", "71" };
            if (coastalParishes.Any(zip => quote.ZipCode.StartsWith(zip)))
            {
                result.Warnings.Add("Coastal property may require Louisiana Citizens participation");
                
                // Wind deductible requirements
                if (quote.Deductible < quote.PropertyValue * 0.02m)
                {
                    result.IsCompliant = false;
                    result.Reason = "Louisiana coastal: Minimum 2% hurricane deductible required";
                    return result;
                }
            }
            
            return result;
        }
        
        private ComplianceCheckResult ValidateGeneralCompliance(Quote quote)
        {
            var result = new ComplianceCheckResult { IsCompliant = true };
            
            // General compliance checks for all states
            if (quote.CoverageLimit < 100000)
            {
                result.IsCompliant = false;
                result.Reason = "Coverage limit below company minimum of $100,000";
                return result;
            }
            
            if (quote.Deductible > quote.PropertyValue * 0.10m)
            {
                result.IsCompliant = false;
                result.Reason = "Deductible cannot exceed 10% of property value";
                return result;
            }
            
            return result;
        }
    }
    
    public class ComplianceCheckResult
    {
        public bool IsCompliant { get; set; }
        public string Reason { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
