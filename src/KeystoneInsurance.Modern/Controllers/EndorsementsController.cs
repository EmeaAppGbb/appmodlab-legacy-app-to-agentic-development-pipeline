using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Domain.Entities;
using KeystoneInsurance.Modern.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeystoneInsurance.Modern.Controllers;

[ApiController]
[Route("api/v1/endorsements")]
public class EndorsementsController : ControllerBase
{
    private readonly KeystoneDbContext _db;
    private readonly IPremiumCalculator _premiumCalculator;

    public EndorsementsController(KeystoneDbContext db, IPremiumCalculator premiumCalculator)
    {
        _db = db;
        _premiumCalculator = premiumCalculator;
    }

    [HttpPost("coverage-change")]
    public async Task<IActionResult> CoverageChange([FromBody] CoverageChangeRequest request, CancellationToken ct)
    {
        var policy = await _db.Policies.Include(p => p.Quote)
            .FirstOrDefaultAsync(p => p.PolicyId == request.PolicyId, ct);
        if (policy is null) return NotFound();

        var limitRatio = request.NewCoverageLimit / policy.CoverageLimit;
        var daysRemaining = (int)(policy.ExpirationDate - request.EffectiveDate).TotalDays;
        var premiumChange = Math.Round(
            policy.AnnualPremium * (limitRatio - 1m) * daysRemaining / 365m, 2);

        var endorsement = new Endorsement
        {
            PolicyId = policy.PolicyId,
            EndorsementNumber = GenerateEndorsementNumber(policy.PolicyNumber),
            EffectiveDate = request.EffectiveDate,
            RequestDate = DateTime.UtcNow,
            EndorsementType = "CoverageChange",
            Status = "Pending",
            ChangeDescription = $"Coverage limit change from {policy.CoverageLimit:C0} to {request.NewCoverageLimit:C0}",
            PremiumChange = premiumChange,
            NewCoverageLimit = request.NewCoverageLimit,
            CreatedDate = DateTime.UtcNow
        };

        _db.Endorsements.Add(endorsement);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetEndorsements), new { policyId = policy.PolicyId }, new
        {
            endorsement.EndorsementId,
            endorsement.EndorsementNumber,
            endorsement.PolicyId,
            endorsement.EndorsementType,
            endorsement.Status,
            endorsement.EffectiveDate,
            endorsement.ChangeDescription,
            endorsement.PremiumChange,
            endorsement.NewCoverageLimit
        });
    }

    [HttpPost("deductible-change")]
    public async Task<IActionResult> DeductibleChange([FromBody] DeductibleChangeRequest request, CancellationToken ct)
    {
        var policy = await _db.Policies.FirstOrDefaultAsync(p => p.PolicyId == request.PolicyId, ct);
        if (policy is null) return NotFound();

        var currentCredit = _premiumCalculator.GetDeductibleCredit(policy.Deductible,
            policy.Quote?.PropertyValue ?? policy.CoverageLimit);
        var newCredit = _premiumCalculator.GetDeductibleCredit(request.NewDeductible,
            policy.Quote?.PropertyValue ?? policy.CoverageLimit);

        var daysRemaining = (int)(policy.ExpirationDate - request.EffectiveDate).TotalDays;
        var premiumChange = Math.Round(
            policy.AnnualPremium * (newCredit / currentCredit - 1m) * daysRemaining / 365m, 2);

        var endorsement = new Endorsement
        {
            PolicyId = policy.PolicyId,
            EndorsementNumber = GenerateEndorsementNumber(policy.PolicyNumber),
            EffectiveDate = request.EffectiveDate,
            RequestDate = DateTime.UtcNow,
            EndorsementType = "CoverageChange",
            Status = "Pending",
            ChangeDescription = $"Deductible change from {policy.Deductible:C0} to {request.NewDeductible:C0}",
            PremiumChange = premiumChange,
            NewDeductible = request.NewDeductible,
            CreatedDate = DateTime.UtcNow
        };

        _db.Endorsements.Add(endorsement);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetEndorsements), new { policyId = policy.PolicyId }, new
        {
            endorsement.EndorsementId,
            endorsement.EndorsementNumber,
            endorsement.PolicyId,
            endorsement.EndorsementType,
            endorsement.Status,
            endorsement.EffectiveDate,
            endorsement.ChangeDescription,
            endorsement.PremiumChange,
            endorsement.NewDeductible
        });
    }

    [HttpPost("cancellation")]
    public async Task<IActionResult> CancellationEndorsement([FromBody] CancellationEndorsementRequest request, CancellationToken ct)
    {
        var policy = await _db.Policies.FirstOrDefaultAsync(p => p.PolicyId == request.PolicyId, ct);
        if (policy is null) return NotFound();

        var daysRemaining = (int)(policy.ExpirationDate - request.CancellationDate).TotalDays;
        // EN-020/EN-021: Insured Request = ShortRate, else ProRata
        var cancellationType = request.Reason == "Insured Request" ? "ShortRate" : "ProRata";
        var returnPremium = _premiumCalculator.CalculateReturnPremium(
            policy.AnnualPremium, Math.Max(daysRemaining, 0), cancellationType);

        var endorsement = new Endorsement
        {
            PolicyId = policy.PolicyId,
            EndorsementNumber = GenerateEndorsementNumber(policy.PolicyNumber),
            EffectiveDate = request.CancellationDate,
            RequestDate = DateTime.UtcNow,
            EndorsementType = "Cancellation",
            Status = "Pending",
            ChangeDescription = $"Policy cancellation effective {request.CancellationDate:d}. Reason: {request.Reason}",
            PremiumChange = -returnPremium,
            CreatedDate = DateTime.UtcNow
        };

        _db.Endorsements.Add(endorsement);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetEndorsements), new { policyId = policy.PolicyId }, new
        {
            endorsement.EndorsementId,
            endorsement.EndorsementNumber,
            endorsement.PolicyId,
            endorsement.EndorsementType,
            endorsement.Status,
            endorsement.EffectiveDate,
            endorsement.ChangeDescription,
            endorsement.PremiumChange
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetEndorsements(
        [FromQuery] int? policyId, [FromQuery] string? status, CancellationToken ct)
    {
        var query = _db.Endorsements.AsQueryable();
        if (policyId.HasValue) query = query.Where(e => e.PolicyId == policyId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.Status == status);

        var items = await query.OrderByDescending(e => e.CreatedDate).ToListAsync(ct);
        return Ok(items);
    }

    private static string GenerateEndorsementNumber(string policyNumber)
    {
        return $"{policyNumber}-END{DateTime.UtcNow:yyyyMMddHHmmss}";
    }
}

public record CoverageChangeRequest
{
    public int PolicyId { get; init; }
    public decimal NewCoverageLimit { get; init; }
    public DateTime EffectiveDate { get; init; }
}

public record DeductibleChangeRequest
{
    public int PolicyId { get; init; }
    public decimal NewDeductible { get; init; }
    public DateTime EffectiveDate { get; init; }
}

public record CancellationEndorsementRequest
{
    public int PolicyId { get; init; }
    public DateTime CancellationDate { get; init; }
    public string Reason { get; init; } = null!;
}
