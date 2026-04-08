using System;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class UnderwritingDecision
    {
        public int UWId { get; set; }
        public int QuoteId { get; set; }
        public int UnderwriterId { get; set; }
        public DateTime DecisionDate { get; set; }
        public string Decision { get; set; } // Approved, Declined, ReferToSenior, RequestMoreInfo
        public decimal RiskScore { get; set; }
        
        // Risk Assessment
        public string CatastropheZoneRating { get; set; }
        public string ConstructionRating { get; set; }
        public string OccupancyRating { get; set; }
        public string ProtectionRating { get; set; }
        public string LossHistoryRating { get; set; }
        
        // Catastrophe Exposure
        public bool HighCatExposure { get; set; }
        public decimal CatastrophePML { get; set; } // Probable Maximum Loss
        
        // Conditions and Stipulations
        public string ApprovalConditions { get; set; }
        public string DeclineReason { get; set; }
        public string AdditionalInformationRequired { get; set; }
        
        // Referral
        public bool ReferredToSeniorUnderwriter { get; set; }
        public int? SeniorUnderwriterId { get; set; }
        public string ReferralReason { get; set; }
        
        public string UnderwritingNotes { get; set; }
        
        // Relationships
        public virtual Quote Quote { get; set; }
    }
}
