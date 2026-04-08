using System;
using KeystoneInsurance.Core.Domain.Entities;

namespace KeystoneInsurance.Core.Services
{
    public class EndorsementService
    {
        private readonly PremiumCalculator _premiumCalculator;
        
        public EndorsementService()
        {
            _premiumCalculator = new PremiumCalculator();
        }
        
        public Endorsement CreateCoverageLimitChange(Policy policy, decimal newLimit, DateTime effectiveDate)
        {
            var endorsement = new Endorsement
            {
                PolicyId = policy.PolicyId,
                EndorsementNumber = GenerateEndorsementNumber(policy.PolicyNumber),
                RequestDate = DateTime.Now,
                EffectiveDate = effectiveDate,
                EndorsementType = "CoverageChange",
                Status = "Pending",
                ChangeDescription = $"Coverage limit change from ${policy.CoverageLimit:N0} to ${newLimit:N0}",
                NewCoverageLimit = newLimit
            };
            
            // Calculate pro-rated premium adjustment
            decimal premiumChange = CalculatePremiumAdjustment(
                policy.AnnualPremium, 
                policy.CoverageLimit, 
                newLimit, 
                effectiveDate, 
                policy.ExpirationDate);
            
            endorsement.PremiumChange = premiumChange;
            
            return endorsement;
        }
        
        public Endorsement CreateDeductibleChange(Policy policy, decimal newDeductible, DateTime effectiveDate)
        {
            var endorsement = new Endorsement
            {
                PolicyId = policy.PolicyId,
                EndorsementNumber = GenerateEndorsementNumber(policy.PolicyNumber),
                RequestDate = DateTime.Now,
                EffectiveDate = effectiveDate,
                EndorsementType = "CoverageChange",
                Status = "Pending",
                ChangeDescription = $"Deductible change from ${policy.Deductible:N0} to ${newDeductible:N0}",
                NewDeductible = newDeductible
            };
            
            // Higher deductible = lower premium, lower deductible = higher premium
            decimal deductibleFactor = CalculateDeductibleFactor(policy.Deductible, newDeductible, policy.CoverageLimit);
            decimal premiumChange = policy.AnnualPremium * deductibleFactor;
            
            // Pro-rate for remaining term
            decimal proratedChange = _premiumCalculator.CalculateProratedPremium(
                premiumChange, effectiveDate, policy.ExpirationDate);
            
            endorsement.PremiumChange = proratedChange;
            
            return endorsement;
        }
        
        public Endorsement CreateCancellationEndorsement(Policy policy, DateTime cancellationDate, string reason)
        {
            var endorsement = new Endorsement
            {
                PolicyId = policy.PolicyId,
                EndorsementNumber = GenerateEndorsementNumber(policy.PolicyNumber),
                RequestDate = DateTime.Now,
                EffectiveDate = cancellationDate,
                EndorsementType = "Cancellation",
                Status = "Pending",
                ChangeDescription = $"Policy cancellation effective {cancellationDate:d}. Reason: {reason}"
            };
            
            // Calculate return premium
            decimal returnPremium = _premiumCalculator.CalculateReturnPremium(
                policy.AnnualPremium, 
                policy.EffectiveDate, 
                cancellationDate, 
                reason == "Insured Request" ? "ShortRate" : "ProRata");
            
            endorsement.PremiumChange = -returnPremium; // Negative for return
            
            return endorsement;
        }
        
        private string GenerateEndorsementNumber(string policyNumber)
        {
            return $"{policyNumber}-END{DateTime.Now:yyyyMMddHHmmss}";
        }
        
        private decimal CalculatePremiumAdjustment(decimal currentPremium, decimal currentLimit, 
            decimal newLimit, DateTime effectiveDate, DateTime expirationDate)
        {
            decimal limitRatio = newLimit / currentLimit;
            decimal newPremium = currentPremium * limitRatio;
            decimal premiumDifference = newPremium - currentPremium;
            
            // Pro-rate for remaining term
            return _premiumCalculator.CalculateProratedPremium(
                premiumDifference, effectiveDate, expirationDate);
        }
        
        private decimal CalculateDeductibleFactor(decimal currentDeductible, decimal newDeductible, decimal coverageLimit)
        {
            decimal currentPercentage = currentDeductible / coverageLimit;
            decimal newPercentage = newDeductible / coverageLimit;
            
            // Simplified deductible credit calculation
            decimal currentCredit = GetDeductibleCredit(currentPercentage);
            decimal newCredit = GetDeductibleCredit(newPercentage);
            
            return (newCredit / currentCredit) - 1.0m;
        }
        
        private decimal GetDeductibleCredit(decimal deductiblePercentage)
        {
            if (deductiblePercentage >= 0.05m) return 0.70m;
            if (deductiblePercentage >= 0.03m) return 0.80m;
            if (deductiblePercentage >= 0.02m) return 0.85m;
            if (deductiblePercentage >= 0.01m) return 0.90m;
            if (deductiblePercentage >= 0.005m) return 0.95m;
            return 1.00m;
        }
    }
}
