using KeystoneInsurance.Modern.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeystoneInsurance.Modern.Controllers;

[ApiController]
[Route("api/v1/policies")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpPost]
    public async Task<IActionResult> IssuePolicy([FromBody] IssuePolicyRequest request, CancellationToken ct)
    {
        var policy = await _policyService.IssuePolicyAsync(
            request.QuoteId, request.EffectiveDate, request.PaymentPlan, ct);

        return CreatedAtAction(nameof(GetPolicy), new { policyId = policy.PolicyId }, new
        {
            policy.PolicyId,
            policy.PolicyNumber,
            policy.EffectiveDate,
            policy.ExpirationDate,
            policy.IssueDate,
            policy.Status,
            policy.AnnualPremium,
            policy.PaymentPlan,
            policy.InstallmentAmount,
            policy.NextPaymentDue,
            policy.CoverageLimit,
            policy.Deductible,
            policy.ReinsuranceCeded,
            policy.CededPremium
        });
    }

    [HttpGet("{policyId:int}")]
    public async Task<IActionResult> GetPolicy(int policyId, CancellationToken ct)
    {
        var policy = await _policyService.GetByIdAsync(policyId, ct);
        if (policy is null) return NotFound();
        return Ok(policy);
    }

    [HttpPost("{policyId:int}/cancel")]
    public async Task<IActionResult> CancelPolicy(int policyId, [FromBody] CancelPolicyRequest request, CancellationToken ct)
    {
        var policy = await _policyService.CancelPolicyAsync(
            policyId, request.CancellationDate, request.CancellationReason, request.CancellationType, ct);

        return Ok(new
        {
            policy.PolicyId,
            policy.Status,
            policy.CancellationDate,
            policy.CancellationReason,
            policy.ReturnPremium
        });
    }
}

public record IssuePolicyRequest
{
    public int QuoteId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string PaymentPlan { get; init; } = "Annual";
}

public record CancelPolicyRequest
{
    public DateTime CancellationDate { get; init; }
    public string CancellationReason { get; init; } = null!;
    public string CancellationType { get; init; } = "ProRata";
}
