using KeystoneInsurance.Modern.Services;
using Microsoft.AspNetCore.Mvc;

namespace KeystoneInsurance.Modern.Controllers;

[ApiController]
[Route("api/v1/underwriting")]
public class UnderwritingController : ControllerBase
{
    private readonly IUnderwritingService _underwritingService;

    public UnderwritingController(IUnderwritingService underwritingService)
    {
        _underwritingService = underwritingService;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> Evaluate([FromBody] EvaluateRequest request, CancellationToken ct)
    {
        // Default underwriter ID; in production this comes from the JWT claims
        var underwriterId = request.UnderwriterId ?? 1;

        var decision = await _underwritingService.EvaluateAsync(request.QuoteId, underwriterId, ct);

        return Ok(new
        {
            decision.UWId,
            decision.QuoteId,
            decision.DecisionDate,
            decision.Decision,
            decision.RiskScore,
            Ratings = new
            {
                decision.ConstructionRating,
                decision.OccupancyRating,
                decision.ProtectionRating,
                decision.LossHistoryRating,
                decision.CatastropheZoneRating
            },
            CatastropheExposure = new
            {
                decision.HighCatExposure,
                decision.CatastrophePML
            },
            decision.ApprovalConditions,
            decision.DeclineReason,
            decision.ReferredToSeniorUnderwriter,
            decision.ReferralReason,
            decision.AdditionalInformationRequired,
            Notes = decision.UnderwritingNotes
        });
    }
}

public record EvaluateRequest
{
    public int QuoteId { get; init; }
    public int? UnderwriterId { get; init; }
}
