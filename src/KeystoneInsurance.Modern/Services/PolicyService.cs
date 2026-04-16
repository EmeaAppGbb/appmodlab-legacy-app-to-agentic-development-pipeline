using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Domain.Entities;
using KeystoneInsurance.Modern.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KeystoneInsurance.Modern.Services;

public interface IPolicyService
{
    Task<Policy> IssuePolicyAsync(int quoteId, DateTime effectiveDate, string paymentPlan, CancellationToken ct = default);
    Task<Policy> CancelPolicyAsync(int policyId, DateTime cancellationDate, string reason, string cancellationType, CancellationToken ct = default);
    Task<Policy?> GetByIdAsync(int policyId, CancellationToken ct = default);
}

public class PolicyService : IPolicyService
{
    private readonly KeystoneDbContext _db;
    private readonly IPremiumCalculator _premiumCalculator;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(KeystoneDbContext db, IPremiumCalculator premiumCalculator, ILogger<PolicyService> logger)
    {
        _db = db;
        _premiumCalculator = premiumCalculator;
        _logger = logger;
    }

    public async Task<Policy> IssuePolicyAsync(int quoteId, DateTime effectiveDate, string paymentPlan, CancellationToken ct = default)
    {
        var quote = await _db.Quotes.Include(q => q.UnderwritingDecision)
            .FirstOrDefaultAsync(q => q.QuoteId == quoteId, ct)
            ?? throw new NotFoundException($"Quote {quoteId} not found");

        if (quote.Status != "Approved")
            throw new InvalidOperationException("Only approved quotes can be bound to policies");

        var policyNumber = PolicyNumber.Generate(effectiveDate);
        var annualPremium = quote.TotalPremium ?? 0m;
        var installment = _premiumCalculator.CalculateInstallmentAmount(annualPremium, paymentPlan);

        // Determine reinsurance cession (PO-005, PO-006)
        var reinsuranceCeded = quote.PropertyValue > 2_000_000m;
        var cededPremium = reinsuranceCeded ? Math.Round(annualPremium * 0.60m, 2) : 0m;

        var policy = new Policy
        {
            QuoteId = quoteId,
            PolicyNumber = policyNumber.Value,
            EffectiveDate = effectiveDate,
            ExpirationDate = effectiveDate.AddYears(1),
            IssueDate = DateTime.UtcNow,
            Status = "Active",
            AnnualPremium = annualPremium,
            PaymentPlan = paymentPlan,
            InstallmentAmount = installment,
            NextPaymentDue = CalculateNextPaymentDue(effectiveDate, paymentPlan),
            CoverageLimit = quote.CoverageLimit,
            Deductible = quote.Deductible,
            BusinessInterruptionCoverage = quote.BusinessInterruptionCoverage,
            BusinessInterruptionLimit = quote.BusinessInterruptionLimit,
            EquipmentBreakdownCoverage = quote.EquipmentBreakdownCoverage,
            FloodCoverage = quote.FloodCoverage,
            EarthquakeCoverage = quote.EarthquakeCoverage,
            ReinsuranceCeded = reinsuranceCeded,
            CededPremium = reinsuranceCeded ? cededPremium : null,
            CreatedDate = DateTime.UtcNow
        };

        quote.Status = "Bound";

        _db.Policies.Add(policy);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Issued policy {PolicyNumber} from quote {QuoteId}", policy.PolicyNumber, quoteId);

        return policy;
    }

    public async Task<Policy> CancelPolicyAsync(int policyId, DateTime cancellationDate, string reason, string cancellationType, CancellationToken ct = default)
    {
        var policy = await _db.Policies.FindAsync([policyId], ct)
            ?? throw new NotFoundException($"Policy {policyId} not found");

        if (policy.Status != "Active")
            throw new InvalidOperationException("Only active policies can be cancelled");

        var daysRemaining = (int)(policy.ExpirationDate - cancellationDate).TotalDays;
        var returnPremium = _premiumCalculator.CalculateReturnPremium(
            policy.AnnualPremium, Math.Max(daysRemaining, 0), cancellationType);

        policy.Status = "Cancelled";
        policy.CancellationDate = cancellationDate;
        policy.CancellationReason = reason;
        policy.ReturnPremium = returnPremium;
        policy.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Cancelled policy {PolicyId}, return premium: {ReturnPremium}",
            policyId, returnPremium);

        return policy;
    }

    public async Task<Policy?> GetByIdAsync(int policyId, CancellationToken ct = default)
    {
        return await _db.Policies
            .Include(p => p.Quote)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.PolicyId == policyId, ct);
    }

    private static DateTime CalculateNextPaymentDue(DateTime effectiveDate, string paymentPlan)
    {
        return paymentPlan switch
        {
            "SemiAnnual" => effectiveDate.AddMonths(6),
            "Quarterly" => effectiveDate.AddMonths(3),
            "Monthly" => effectiveDate.AddMonths(1),
            _ => effectiveDate.AddYears(1) // Annual - no next payment
        };
    }
}
