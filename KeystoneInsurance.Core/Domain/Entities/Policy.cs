using System;
using System.Collections.Generic;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class Policy
    {
        public int PolicyId { get; set; }
        public int QuoteId { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime IssueDate { get; set; }
        public string Status { get; set; } // Active, Cancelled, Expired, PendingCancellation, Renewed
        
        // Premium and Payment
        public decimal AnnualPremium { get; set; }
        public string PaymentPlan { get; set; } // Annual, SemiAnnual, Quarterly, Monthly
        public decimal InstallmentAmount { get; set; }
        public DateTime NextPaymentDue { get; set; }
        
        // Coverage
        public decimal CoverageLimit { get; set; }
        public decimal Deductible { get; set; }
        public string CoverageType { get; set; }
        
        // Additional Coverages
        public bool BusinessInterruptionCoverage { get; set; }
        public decimal BusinessInterruptionLimit { get; set; }
        public bool EquipmentBreakdownCoverage { get; set; }
        public bool FloodCoverage { get; set; }
        public decimal FloodLimit { get; set; }
        public bool EarthquakeCoverage { get; set; }
        public decimal EarthquakeLimit { get; set; }
        
        // Reinsurance
        public bool ReinsuranceCeded { get; set; }
        public decimal CededPremium { get; set; }
        public string ReinsuranceTreatyId { get; set; }
        
        // Cancellation
        public DateTime? CancellationDate { get; set; }
        public string CancellationReason { get; set; }
        public decimal? ReturnPremium { get; set; }
        
        // Documents
        public string PolicyDocumentPath { get; set; }
        public DateTime? DocumentGeneratedDate { get; set; }
        
        // Relationships
        public virtual Quote Quote { get; set; }
        public virtual ICollection<Endorsement> Endorsements { get; set; }
    }
}
