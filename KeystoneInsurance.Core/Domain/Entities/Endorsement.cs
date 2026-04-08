using System;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class Endorsement
    {
        public int EndorsementId { get; set; }
        public int PolicyId { get; set; }
        public string EndorsementNumber { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime RequestDate { get; set; }
        public string EndorsementType { get; set; } // CoverageChange, PremiumAdjustment, NameChange, AddressChange, Cancellation
        public string Status { get; set; }
        
        // Changes
        public string ChangeDescription { get; set; }
        public decimal? PremiumChange { get; set; }
        public decimal? NewCoverageLimit { get; set; }
        public decimal? NewDeductible { get; set; }
        
        // Approval
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        
        // Documents
        public string EndorsementDocumentPath { get; set; }
        
        // Relationships
        public virtual Policy Policy { get; set; }
    }
}
