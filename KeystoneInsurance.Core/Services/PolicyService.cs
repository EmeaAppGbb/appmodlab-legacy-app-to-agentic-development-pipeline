using System;
using System.Collections.Generic;
using KeystoneInsurance.Core.Domain.Entities;

namespace KeystoneInsurance.Core.Services
{
    public class PolicyService
    {
        private readonly PremiumCalculator _premiumCalculator;
        
        public PolicyService()
        {
            _premiumCalculator = new PremiumCalculator();
        }
        
        public Policy IssuePolicy(Quote approvedQuote, DateTime effectiveDate, string paymentPlan)
        {
            if (approvedQuote.Status != "Approved")
                throw new InvalidOperationException("Only approved quotes can be bound to policies");
            
            var policy = new Policy
            {
                QuoteId = approvedQuote.QuoteId,
                PolicyNumber = GeneratePolicyNumber(),
                EffectiveDate = effectiveDate,
                ExpirationDate = effectiveDate.AddYears(1),
                IssueDate = DateTime.Now,
                Status = "Active",
                
                // Premium details
                AnnualPremium = approvedQuote.TotalPremium,
                PaymentPlan = paymentPlan,
                InstallmentAmount = _premiumCalculator.CalculateInstallmentAmount(approvedQuote.TotalPremium, paymentPlan),
                NextPaymentDue = CalculateNextPaymentDate(effectiveDate, paymentPlan),
                
                // Coverage details
                CoverageLimit = approvedQuote.CoverageLimit,
                Deductible = approvedQuote.Deductible,
                CoverageType = "Commercial Property",
                
                // Optional coverages
                BusinessInterruptionCoverage = approvedQuote.BusinessInterruptionCoverage,
                BusinessInterruptionLimit = approvedQuote.BusinessInterruptionLimit,
                EquipmentBreakdownCoverage = approvedQuote.EquipmentBreakdownCoverage,
                FloodCoverage = approvedQuote.FloodCoverage,
                FloodLimit = approvedQuote.FloodCoverage ? approvedQuote.CoverageLimit : 0,
                EarthquakeCoverage = approvedQuote.EarthquakeCoverage,
                EarthquakeLimit = approvedQuote.EarthquakeCoverage ? approvedQuote.CoverageLimit : 0,
                
                // Reinsurance
                ReinsuranceCeded = DetermineReinsuranceCession(approvedQuote),
                CededPremium = CalculateCededPremium(approvedQuote)
            };
            
            return policy;
        }
        
        public Policy CancelPolicy(Policy policy, DateTime cancellationDate, string cancellationReason, 
            string cancellationType)
        {
            if (policy.Status != "Active")
                throw new InvalidOperationException("Only active policies can be cancelled");
                
            policy.Status = "Cancelled";
            policy.CancellationDate = cancellationDate;
            policy.CancellationReason = cancellationReason;
            policy.ReturnPremium = _premiumCalculator.CalculateReturnPremium(
                policy.AnnualPremium, 
                policy.EffectiveDate, 
                cancellationDate, 
                cancellationType);
            
            return policy;
        }
        
        public Policy RenewPolicy(Policy expiringPolicy, decimal newPremium)
        {
            if (expiringPolicy.Status != "Active")
                throw new InvalidOperationException("Only active policies can be renewed");
                
            var renewalPolicy = new Policy
            {
                PolicyNumber = GeneratePolicyNumber(),
                EffectiveDate = expiringPolicy.ExpirationDate,
                ExpirationDate = expiringPolicy.ExpirationDate.AddYears(1),
                IssueDate = DateTime.Now,
                Status = "Active",
                
                // Copy coverage from expiring policy
                AnnualPremium = newPremium,
                PaymentPlan = expiringPolicy.PaymentPlan,
                InstallmentAmount = _premiumCalculator.CalculateInstallmentAmount(newPremium, expiringPolicy.PaymentPlan),
                NextPaymentDue = expiringPolicy.ExpirationDate,
                CoverageLimit = expiringPolicy.CoverageLimit,
                Deductible = expiringPolicy.Deductible,
                CoverageType = expiringPolicy.CoverageType,
                
                BusinessInterruptionCoverage = expiringPolicy.BusinessInterruptionCoverage,
                BusinessInterruptionLimit = expiringPolicy.BusinessInterruptionLimit,
                EquipmentBreakdownCoverage = expiringPolicy.EquipmentBreakdownCoverage,
                FloodCoverage = expiringPolicy.FloodCoverage,
                FloodLimit = expiringPolicy.FloodLimit,
                EarthquakeCoverage = expiringPolicy.EarthquakeCoverage,
                EarthquakeLimit = expiringPolicy.EarthquakeLimit
            };
            
            // Mark old policy as renewed
            expiringPolicy.Status = "Renewed";
            
            return renewalPolicy;
        }
        
        private string GeneratePolicyNumber()
        {
            return $"KIP{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 10).ToUpper()}";
        }
        
        private DateTime CalculateNextPaymentDate(DateTime effectiveDate, string paymentPlan)
        {
            switch (paymentPlan)
            {
                case "Annual":
                    return effectiveDate.AddYears(1);
                case "SemiAnnual":
                    return effectiveDate.AddMonths(6);
                case "Quarterly":
                    return effectiveDate.AddMonths(3);
                case "Monthly":
                    return effectiveDate.AddMonths(1);
                default:
                    return effectiveDate;
            }
        }
        
        private bool DetermineReinsuranceCession(Quote quote)
        {
            // Cede to reinsurance if property value exceeds retention limit
            return quote.PropertyValue > 2000000m;
        }
        
        private decimal CalculateCededPremium(Quote quote)
        {
            if (!DetermineReinsuranceCession(quote))
                return 0;
                
            // Cede 60% of premium for high-value properties
            return quote.TotalPremium * 0.60m;
        }
    }
}
