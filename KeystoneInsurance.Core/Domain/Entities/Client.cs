using System;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class Client
    {
        public int ClientId { get; set; }
        public string ClientNumber { get; set; }
        public string BusinessName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        
        // Business Information
        public string BusinessType { get; set; }
        public int YearsInBusiness { get; set; }
        public string FederalTaxId { get; set; }
        
        // Address
        public string MailingAddress { get; set; }
        public string MailingCity { get; set; }
        public string MailingState { get; set; }
        public string MailingZip { get; set; }
        
        // Account Information
        public DateTime AccountCreatedDate { get; set; }
        public string AccountStatus { get; set; }
        public decimal? CreditScore { get; set; }
        
        // Risk Profile
        public string RiskTier { get; set; } // Preferred, Standard, SubStandard
        public int TotalActivePolicies { get; set; }
        public decimal TotalPremiumInForce { get; set; }
        public int ClaimsHistory { get; set; }
    }
}
