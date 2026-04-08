using System;

namespace KeystoneInsurance.Core.Domain.Rules
{
    public class UnderwritingRules
    {
        public static bool IsAutoDecline(int buildingAge, string constructionType, int priorClaimsCount, 
            decimal catastrophePML, decimal propertyValue)
        {
            // Auto-decline if building is old frame construction with poor history
            if (buildingAge > 75 && constructionType == "Frame" && priorClaimsCount >= 3)
                return true;
            
            // Auto-decline if catastrophe PML exceeds 60% of property value
            if (catastrophePML > propertyValue * 0.60m)
                return true;
            
            // Auto-decline if more than 5 prior claims
            if (priorClaimsCount > 5)
                return true;
            
            return false;
        }
        
        public static bool RequiresSeniorUnderwriterReview(decimal propertyValue, int buildingAge, 
            string occupancyType, decimal riskScore)
        {
            // High-value properties need senior review
            if (propertyValue > 5000000m)
                return true;
            
            // Historic buildings need senior review
            if (buildingAge > 100)
                return true;
            
            // High-risk occupancies need senior review
            if (occupancyType == "Manufacturing-Heavy" || occupancyType == "Restaurant")
                return true;
            
            // High risk scores need senior review
            if (riskScore > 70)
                return true;
            
            return false;
        }
        
        public static bool RequiresPropertyInspection(int buildingAge, int roofAge, 
            decimal propertyValue, int priorClaimsCount)
        {
            if (buildingAge > 30)
                return true;
            if (roofAge > 15)
                return true;
            if (propertyValue > 3000000m)
                return true;
            if (priorClaimsCount >= 2)
                return true;
                
            return false;
        }
    }
}
