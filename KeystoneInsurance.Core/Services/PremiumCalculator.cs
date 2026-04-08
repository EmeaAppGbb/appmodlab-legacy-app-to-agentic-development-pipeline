using System;

namespace KeystoneInsurance.Core.Services
{
    public class PremiumCalculator
    {
        public decimal CalculateProratedPremium(decimal annualPremium, DateTime effectiveDate, DateTime expirationDate)
        {
            var totalDays = (expirationDate - effectiveDate).TotalDays;
            var dailyRate = annualPremium / 365;
            return (decimal)(dailyRate * totalDays);
        }
        
        public decimal CalculateReturnPremium(decimal annualPremium, DateTime effectiveDate, 
            DateTime cancellationDate, string cancellationType)
        {
            var daysUsed = (cancellationDate - effectiveDate).TotalDays;
            var totalDays = 365;
            
            if (cancellationType == "ProRata")
            {
                // Pro-rata cancellation - return unused premium
                var daysRemaining = totalDays - daysUsed;
                return annualPremium * ((decimal)daysRemaining / totalDays);
            }
            else if (cancellationType == "ShortRate")
            {
                // Short-rate cancellation - apply 10% penalty
                var daysRemaining = totalDays - daysUsed;
                var returnPremium = annualPremium * ((decimal)daysRemaining / totalDays);
                return returnPremium * 0.90m; // 10% penalty
            }
            else
            {
                // Flat cancellation - no return premium
                return 0;
            }
        }
        
        public decimal CalculateInstallmentAmount(decimal annualPremium, string paymentPlan)
        {
            switch (paymentPlan)
            {
                case "Annual":
                    return annualPremium;
                case "SemiAnnual":
                    return (annualPremium / 2) * 1.03m; // 3% installment fee
                case "Quarterly":
                    return (annualPremium / 4) * 1.05m; // 5% installment fee
                case "Monthly":
                    return (annualPremium / 12) * 1.08m; // 8% installment fee
                default:
                    return annualPremium;
            }
        }
    }
}
