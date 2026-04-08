using System;
using System.Collections.Generic;
using KeystoneInsurance.Core.Domain.Entities;
using KeystoneInsurance.Core.Domain.Rules;

namespace KeystoneInsurance.Core.Services
{
    public class UnderwritingService
    {
        private readonly UnderwritingRules _rules;
        
        public UnderwritingService()
        {
            _rules = new UnderwritingRules();
        }
        
        public UnderwritingDecision EvaluateQuote(Quote quote)
        {
            var decision = new UnderwritingDecision
            {
                QuoteId = quote.QuoteId,
                DecisionDate = DateTime.Now,
                UnderwriterId = 1 // Would come from current user context
            };
            
            // Calculate risk score (0-100, higher = more risk)
            decimal riskScore = CalculateRiskScore(quote);
            decision.RiskScore = riskScore;
            
            // Evaluate each risk component
            decision.ConstructionRating = EvaluateConstruction(quote.ConstructionType, quote.YearBuilt);
            decision.OccupancyRating = EvaluateOccupancy(quote.OccupancyType);
            decision.ProtectionRating = EvaluateProtection(quote);
            decision.LossHistoryRating = EvaluateLossHistory(quote.PriorClaimsCount, quote.PriorClaimsTotalAmount);
            decision.CatastropheZoneRating = EvaluateCatastropheExposure(quote.StateCode, quote.ZipCode);
            
            // Calculate Probable Maximum Loss
            decision.CatastrophePML = CalculatePML(quote);
            
            // Determine if high cat exposure
            decision.HighCatExposure = decision.CatastrophePML > quote.PropertyValue * 0.50m;
            
            // Make underwriting decision based on risk score and specific criteria
            if (riskScore > 85 || decision.HighCatExposure)
            {
                decision.Decision = "Declined";
                decision.DeclineReason = BuildDeclineReason(riskScore, decision);
            }
            else if (riskScore > 70 || quote.PriorClaimsCount >= 3)
            {
                decision.Decision = "ReferToSenior";
                decision.ReferredToSeniorUnderwriter = true;
                decision.ReferralReason = BuildReferralReason(riskScore, quote);
            }
            else if (riskScore > 60 || NeedsMoreInformation(quote))
            {
                decision.Decision = "RequestMoreInfo";
                decision.AdditionalInformationRequired = BuildInfoRequest(quote);
            }
            else
            {
                decision.Decision = "Approved";
                decision.ApprovalConditions = BuildApprovalConditions(quote, riskScore);
            }
            
            decision.UnderwritingNotes = $"Risk Score: {riskScore:F2}. Evaluated {DateTime.Now:g}";
            
            return decision;
        }
        
        private decimal CalculateRiskScore(Quote quote)
        {
            decimal score = 50; // Base score
            
            // Construction type risk
            switch (quote.ConstructionType)
            {
                case "Frame":
                    score += 15;
                    break;
                case "Joisted Masonry":
                    score += 10;
                    break;
                case "Non-Combustible":
                    score += 5;
                    break;
                case "Fire Resistive":
                    score -= 5;
                    break;
            }
            
            // Age risk
            int buildingAge = DateTime.Now.Year - quote.YearBuilt;
            if (buildingAge > 50)
                score += 15;
            else if (buildingAge > 30)
                score += 10;
            else if (buildingAge > 20)
                score += 5;
            else if (buildingAge < 5)
                score -= 5;
            
            // Occupancy risk
            if (quote.OccupancyType == "Restaurant")
                score += 12;
            else if (quote.OccupancyType == "Manufacturing-Heavy")
                score += 15;
            else if (quote.OccupancyType == "Office")
                score -= 5;
            
            // Protection credits
            if (quote.SprinklersInstalled)
                score -= 10;
            if (quote.AlarmSystemInstalled)
                score -= 5;
            
            // Loss history
            score += quote.PriorClaimsCount * 8;
            if (quote.PriorClaimsTotalAmount > 100000)
                score += 10;
            
            // Catastrophe exposure
            var highCatStates = new[] { "FL", "CA", "LA", "TX" };
            if (Array.Exists(highCatStates, s => s == quote.StateCode))
                score += 10;
            
            // Roof condition
            if (quote.RoofAge > 20)
                score += 12;
            else if (quote.RoofAge < 5)
                score -= 3;
            
            // Property value considerations
            if (quote.PropertyValue > 5000000)
                score += 8; // High-value property needs senior review
            
            return Math.Max(0, Math.Min(100, score));
        }
        
        private string EvaluateConstruction(string constructionType, int yearBuilt)
        {
            int age = DateTime.Now.Year - yearBuilt;
            
            if (constructionType == "Fire Resistive" && age < 20)
                return "Excellent";
            else if (constructionType == "Frame" && age > 50)
                return "Poor";
            else if (age < 10)
                return "Good";
            else if (age > 40)
                return "Fair";
            else
                return "Average";
        }
        
        private string EvaluateOccupancy(string occupancyType)
        {
            var lowRisk = new[] { "Office", "Educational", "Warehouse" };
            var highRisk = new[] { "Restaurant", "Manufacturing-Heavy", "Hotel" };
            
            if (Array.Exists(lowRisk, o => o == occupancyType))
                return "Low Risk";
            else if (Array.Exists(highRisk, o => o == occupancyType))
                return "High Risk";
            else
                return "Average Risk";
        }
        
        private string EvaluateProtection(Quote quote)
        {
            if (quote.SprinklersInstalled && quote.AlarmSystemInstalled)
                return "Superior";
            else if (quote.SprinklersInstalled || quote.AlarmSystemInstalled)
                return "Good";
            else
                return "Basic";
        }
        
        private string EvaluateLossHistory(int claimsCount, decimal claimsAmount)
        {
            if (claimsCount == 0)
                return "Loss Free";
            else if (claimsCount == 1 && claimsAmount < 25000)
                return "Favorable";
            else if (claimsCount <= 2 && claimsAmount < 100000)
                return "Average";
            else if (claimsCount >= 3 || claimsAmount > 250000)
                return "Poor";
            else
                return "Below Average";
        }
        
        private string EvaluateCatastropheExposure(string stateCode, string zipCode)
        {
            var extremeCatStates = new[] { "FL", "LA" };
            var highCatStates = new[] { "CA", "TX", "NC", "SC" };
            
            if (Array.Exists(extremeCatStates, s => s == stateCode))
                return "Extreme";
            else if (Array.Exists(highCatStates, s => s == stateCode))
                return "High";
            else
                return "Moderate";
        }
        
        private decimal CalculatePML(Quote quote)
        {
            // Probable Maximum Loss calculation
            decimal pml = quote.PropertyValue * 0.25m; // Base 25% PML
            
            // Adjust for catastrophe exposure
            if (quote.StateCode == "FL" || quote.StateCode == "LA")
                pml = quote.PropertyValue * 0.60m;
            else if (quote.StateCode == "CA")
                pml = quote.PropertyValue * 0.50m;
            
            // Adjust for protection
            if (quote.SprinklersInstalled)
                pml *= 0.70m;
            
            return pml;
        }
        
        private bool NeedsMoreInformation(Quote quote)
        {
            // Check if critical information is missing or unclear
            if (quote.RoofAge > 20 && quote.RoofType == null)
                return true;
            if (quote.PriorClaimsCount > 0 && quote.PriorClaimsTotalAmount == 0)
                return true;
            if (quote.PropertyValue > 2000000 && quote.SquareFootage == 0)
                return true;
                
            return false;
        }
        
        private string BuildDeclineReason(decimal riskScore, UnderwritingDecision decision)
        {
            var reasons = new List<string>();
            
            if (riskScore > 85)
                reasons.Add($"Risk score ({riskScore:F0}) exceeds acceptable threshold");
            if (decision.HighCatExposure)
                reasons.Add($"Catastrophe exposure (PML: ${decision.CatastrophePML:N0}) exceeds appetite");
            if (decision.LossHistoryRating == "Poor")
                reasons.Add("Unfavorable loss history");
            if (decision.ConstructionRating == "Poor")
                reasons.Add("Construction class and age combination unacceptable");
                
            return string.Join("; ", reasons);
        }
        
        private string BuildReferralReason(decimal riskScore, Quote quote)
        {
            var reasons = new List<string>();
            
            if (riskScore > 70)
                reasons.Add($"Risk score ({riskScore:F0}) requires senior review");
            if (quote.PropertyValue > 5000000)
                reasons.Add($"High property value (${quote.PropertyValue:N0})");
            if (quote.PriorClaimsCount >= 3)
                reasons.Add($"Multiple prior claims ({quote.PriorClaimsCount})");
                
            return string.Join("; ", reasons);
        }
        
        private string BuildInfoRequest(Quote quote)
        {
            var requests = new List<string>();
            
            if (quote.RoofAge > 20)
                requests.Add("Recent roof inspection report required");
            if (quote.PriorClaimsCount > 0)
                requests.Add("Detailed loss runs for past 5 years");
            if (quote.PropertyValue > 2000000)
                requests.Add("Current property appraisal");
            if (!quote.SprinklersInstalled && quote.OccupancyType == "Restaurant")
                requests.Add("Fire safety plan and equipment documentation");
                
            return string.Join("; ", requests);
        }
        
        private string BuildApprovalConditions(Quote quote, decimal riskScore)
        {
            var conditions = new List<string>();
            
            if (quote.RoofAge > 15)
                conditions.Add("Roof inspection required within 30 days of binding");
            if (riskScore > 55)
                conditions.Add("Annual property inspections required");
            if (quote.PropertyValue > 1000000)
                conditions.Add("Agreed value settlement basis");
                
            return conditions.Count > 0 ? string.Join("; ", conditions) : "Standard terms apply";
        }
    }
}
