using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Domain.Entities;
using KeystoneInsurance.Modern.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KeystoneInsurance.Modern.Controllers;

[ApiController]
[Route("api/v1/quotes")]
public class QuotesController : ControllerBase
{
    private readonly IQuotingEngine _quotingEngine;
    private readonly KeystoneDbContext _db;

    public QuotesController(IQuotingEngine quotingEngine, KeystoneDbContext db)
    {
        _quotingEngine = quotingEngine;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request, CancellationToken ct)
    {
        var quote = new Quote
        {
            ClientId = request.ClientId,
            PropertyAddress = request.PropertyAddress,
            City = request.City,
            StateCode = request.StateCode,
            ZipCode = request.ZipCode,
            PropertyValue = request.PropertyValue,
            ConstructionType = request.ConstructionType,
            OccupancyType = request.OccupancyType,
            YearBuilt = request.YearBuilt,
            SquareFootage = request.SquareFootage,
            NumberOfStories = request.NumberOfStories,
            SprinklersInstalled = request.SprinklersInstalled,
            AlarmSystemInstalled = request.AlarmSystemInstalled,
            RoofType = request.RoofType,
            RoofAge = request.RoofAge,
            CoverageLimit = request.CoverageLimit,
            Deductible = request.Deductible,
            BusinessInterruptionCoverage = request.BusinessInterruptionCoverage,
            BusinessInterruptionLimit = request.BusinessInterruptionLimit,
            EquipmentBreakdownCoverage = request.EquipmentBreakdownCoverage,
            FloodCoverage = request.FloodCoverage,
            EarthquakeCoverage = request.EarthquakeCoverage,
            PriorClaimsCount = request.PriorClaimsCount,
            PriorClaimsTotalAmount = request.PriorClaimsTotalAmount
        };

        var created = await _quotingEngine.CreateQuoteAsync(quote, ct);
        return CreatedAtAction(nameof(GetQuote), new { quoteId = created.QuoteId }, new
        {
            created.QuoteId,
            created.QuoteNumber,
            created.Status,
            created.CreatedDate,
            created.ExpirationDate,
            created.BasePremium,
            created.TotalPremium,
            created.PremiumCalculationDetails,
            ComplianceWarnings = Array.Empty<string>()
        });
    }

    [HttpGet("{quoteId:int}")]
    public async Task<IActionResult> GetQuote(int quoteId, CancellationToken ct)
    {
        var quote = await _db.Quotes
            .Include(q => q.Client)
            .Include(q => q.UnderwritingDecision)
            .FirstOrDefaultAsync(q => q.QuoteId == quoteId, ct);

        if (quote is null) return NotFound();
        return Ok(quote);
    }

    [HttpGet]
    public async Task<IActionResult> SearchQuotes(
        [FromQuery] int? clientId, [FromQuery] string? stateCode, [FromQuery] string? status,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.Quotes.Include(q => q.Client).AsQueryable();

        if (clientId.HasValue) query = query.Where(q => q.ClientId == clientId.Value);
        if (!string.IsNullOrEmpty(stateCode)) query = query.Where(q => q.StateCode == stateCode);
        if (!string.IsNullOrEmpty(status)) query = query.Where(q => q.Status == status);
        if (fromDate.HasValue) query = query.Where(q => q.CreatedDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(q => q.CreatedDate <= toDate.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(q => q.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new
            {
                q.QuoteId,
                q.QuoteNumber,
                q.Status,
                q.CreatedDate,
                q.PropertyAddress,
                q.City,
                q.StateCode,
                q.TotalPremium,
                ClientName = q.Client.BusinessName
            })
            .ToListAsync(ct);

        return Ok(new { items, page, pageSize, totalCount });
    }

    [HttpPut("{quoteId:int}/recalculate")]
    public async Task<IActionResult> Recalculate(int quoteId, CancellationToken ct)
    {
        var quote = await _quotingEngine.RecalculateAsync(quoteId, ct);
        return Ok(new
        {
            quote.QuoteId,
            quote.QuoteNumber,
            quote.Status,
            quote.CreatedDate,
            quote.ExpirationDate,
            quote.BasePremium,
            quote.TotalPremium,
            quote.PremiumCalculationDetails,
            ComplianceWarnings = Array.Empty<string>()
        });
    }
}

public record CreateQuoteRequest
{
    public int ClientId { get; init; }
    public string PropertyAddress { get; init; } = null!;
    public string City { get; init; } = null!;
    public string StateCode { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
    public decimal PropertyValue { get; init; }
    public string ConstructionType { get; init; } = null!;
    public string OccupancyType { get; init; } = null!;
    public int YearBuilt { get; init; }
    public int SquareFootage { get; init; }
    public int NumberOfStories { get; init; }
    public bool SprinklersInstalled { get; init; }
    public bool AlarmSystemInstalled { get; init; }
    public string? RoofType { get; init; }
    public int RoofAge { get; init; }
    public decimal CoverageLimit { get; init; }
    public decimal Deductible { get; init; }
    public bool BusinessInterruptionCoverage { get; init; }
    public decimal? BusinessInterruptionLimit { get; init; }
    public bool EquipmentBreakdownCoverage { get; init; }
    public bool FloodCoverage { get; init; }
    public bool EarthquakeCoverage { get; init; }
    public int PriorClaimsCount { get; init; }
    public decimal PriorClaimsTotalAmount { get; init; }
}
