using System;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class Coverage
    {
        public int CoverageId { get; set; }
        public string CoverageType { get; set; }
        public string CoverageCode { get; set; }
        public string Description { get; set; }
        public decimal Limit { get; set; }
        public decimal Deductible { get; set; }
        public decimal Premium { get; set; }
        public bool IsOptional { get; set; }
        public bool IsIncluded { get; set; }
        
        // Rating Information
        public decimal BaseRate { get; set; }
        public string RatingBasis { get; set; }
        public decimal RatingFactor { get; set; }
    }
}
