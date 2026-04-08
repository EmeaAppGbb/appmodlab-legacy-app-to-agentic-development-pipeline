using System;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class Quote
    {
        public int QuoteId { get; set; }
        public int ClientId { get; set; }
        public string QuoteNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Status { get; set; } // Draft, Pending, Approved, Declined, Expired, Bound
        
        // Property Information
        public string PropertyAddress { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
        public decimal PropertyValue { get; set; }
        public string ConstructionType { get; set; } // Frame, Joisted Masonry, Non-Combustible, Masonry Non-Combustible, Modified Fire Resistive, Fire Resistive
        public string OccupancyType { get; set; }
        public int YearBuilt { get; set; }
        public int SquareFootage { get; set; }
        public int NumberOfStories { get; set; }
        public bool SprinklersInstalled { get; set; }
        public bool AlarmSystemInstalled { get; set; }
        public string RoofType { get; set; }
        public int RoofAge { get; set; }
        
        // Coverage Details
        public decimal CoverageLimit { get; set; }
        public decimal Deductible { get; set; }
        public bool BusinessInterruptionCoverage { get; set; }
        public decimal BusinessInterruptionLimit { get; set; }
        public bool EquipmentBreakdownCoverage { get; set; }
        public bool FloodCoverage { get; set; }
        public bool EarthquakeCoverage { get; set; }
        
        // Loss History
        public int PriorClaimsCount { get; set; }
        public decimal PriorClaimsTotalAmount { get; set; }
        
        // Premium Calculation Results
        public decimal BasePremium { get; set; }
        public decimal TotalPremium { get; set; }
        public string PremiumCalculationDetails { get; set; }
        
        // Underwriting
        public int? UnderwriterId { get; set; }
        public string UnderwritingNotes { get; set; }
        
        // Relationships
        public virtual Client Client { get; set; }
        public virtual UnderwritingDecision UnderwritingDecision { get; set; }
    }
}
