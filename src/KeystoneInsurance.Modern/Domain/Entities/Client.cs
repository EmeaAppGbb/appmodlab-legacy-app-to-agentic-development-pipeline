namespace KeystoneInsurance.Modern.Domain.Entities;

public class Client
{
    public int ClientId { get; set; }
    public string ClientNumber { get; set; } = null!;
    public string BusinessName { get; set; } = null!;
    public string? ContactFirstName { get; set; }
    public string? ContactLastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    // Business Information
    public string? BusinessType { get; set; }
    public int YearsInBusiness { get; set; }
    public string? FederalTaxId { get; set; }

    // Address
    public string? MailingAddress { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingZip { get; set; }

    // Account Information
    public DateTime AccountCreatedDate { get; set; }
    public string AccountStatus { get; set; } = "Active";
    public decimal? CreditScore { get; set; }

    // Risk Profile
    public string? RiskTier { get; set; }
    public int TotalActivePolicies { get; set; }
    public decimal TotalPremiumInForce { get; set; }
    public int ClaimsHistory { get; set; }

    // Audit
    public string? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
}
