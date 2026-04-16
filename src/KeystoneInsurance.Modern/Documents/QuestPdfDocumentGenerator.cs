using Azure.Storage.Blobs;
using KeystoneInsurance.Modern.Data;
using KeystoneInsurance.Modern.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KeystoneInsurance.Modern.Documents;

/// <summary>
/// QuestPDF-based document generator replacing Crystal Reports.
/// Generates policy declarations, endorsement documents, quote summaries,
/// and renewal offer letters, storing them in Azure Blob Storage.
/// </summary>
public class QuestPdfDocumentGenerator : IPolicyDocumentGenerator
{
    private readonly KeystoneDbContext _dbContext;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<QuestPdfDocumentGenerator> _logger;

    public QuestPdfDocumentGenerator(
        KeystoneDbContext dbContext,
        BlobServiceClient blobServiceClient,
        ILogger<QuestPdfDocumentGenerator> logger)
    {
        _dbContext = dbContext;
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<DocumentResult> GeneratePolicyDocumentAsync(int policyId)
    {
        var policy = await _dbContext.Policies
            .Include(p => p.Quote).ThenInclude(q => q.Client)
            .Include(p => p.Endorsements)
            .FirstOrDefaultAsync(p => p.PolicyId == policyId)
            ?? throw new InvalidOperationException($"Policy {policyId} not found");

        _logger.LogInformation("Generating policy document for {PolicyNumber}", policy.PolicyNumber);

        var pdfBytes = GeneratePolicyPdf(policy);
        var fileName = $"policies/{policy.PolicyNumber}/declaration-{DateTime.UtcNow:yyyyMMdd}.pdf";

        var blobUrl = await UploadToBlobAsync("policy-documents", fileName, pdfBytes);

        policy.PolicyDocumentPath = blobUrl;
        policy.DocumentGeneratedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Policy document generated for {PolicyNumber}: {FileName} ({Size} bytes)",
            policy.PolicyNumber, fileName, pdfBytes.Length);

        return new DocumentResult
        {
            BlobUrl = blobUrl,
            FileName = fileName,
            FileSizeBytes = pdfBytes.Length,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<DocumentResult> GenerateEndorsementDocumentAsync(int endorsementId)
    {
        var endorsement = await _dbContext.Endorsements
            .Include(e => e.Policy).ThenInclude(p => p.Quote).ThenInclude(q => q.Client)
            .FirstOrDefaultAsync(e => e.EndorsementId == endorsementId)
            ?? throw new InvalidOperationException($"Endorsement {endorsementId} not found");

        _logger.LogInformation(
            "Generating endorsement document for {EndorsementNumber}", endorsement.EndorsementNumber);

        var pdfBytes = GenerateEndorsementPdf(endorsement);
        var fileName = $"endorsements/{endorsement.EndorsementNumber}/endorsement-{DateTime.UtcNow:yyyyMMdd}.pdf";

        var blobUrl = await UploadToBlobAsync("endorsement-documents", fileName, pdfBytes);

        endorsement.EndorsementDocumentPath = blobUrl;
        await _dbContext.SaveChangesAsync();

        return new DocumentResult
        {
            BlobUrl = blobUrl,
            FileName = fileName,
            FileSizeBytes = pdfBytes.Length,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<DocumentResult> GenerateQuoteSummaryAsync(int quoteId)
    {
        var quote = await _dbContext.Quotes
            .Include(q => q.Client)
            .FirstOrDefaultAsync(q => q.QuoteId == quoteId)
            ?? throw new InvalidOperationException($"Quote {quoteId} not found");

        _logger.LogInformation("Generating quote summary for {QuoteNumber}", quote.QuoteNumber);

        var pdfBytes = GenerateQuotePdf(quote);
        var fileName = $"quotes/{quote.QuoteNumber}/summary-{DateTime.UtcNow:yyyyMMdd}.pdf";

        var blobUrl = await UploadToBlobAsync("policy-documents", fileName, pdfBytes);

        return new DocumentResult
        {
            BlobUrl = blobUrl,
            FileName = fileName,
            FileSizeBytes = pdfBytes.Length,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<DocumentResult> GenerateRenewalOfferAsync(int renewalPolicyId)
    {
        var policy = await _dbContext.Policies
            .Include(p => p.Quote).ThenInclude(q => q.Client)
            .FirstOrDefaultAsync(p => p.PolicyId == renewalPolicyId)
            ?? throw new InvalidOperationException($"Policy {renewalPolicyId} not found");

        _logger.LogInformation(
            "Generating renewal offer for {PolicyNumber}", policy.PolicyNumber);

        var pdfBytes = GenerateRenewalPdf(policy);
        var fileName = $"policies/{policy.PolicyNumber}/renewal-offer-{DateTime.UtcNow:yyyyMMdd}.pdf";

        var blobUrl = await UploadToBlobAsync("policy-documents", fileName, pdfBytes);

        return new DocumentResult
        {
            BlobUrl = blobUrl,
            FileName = fileName,
            FileSizeBytes = pdfBytes.Length,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<string> UploadToBlobAsync(string containerName, string fileName, byte[] pdfBytes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(new BinaryData(pdfBytes), overwrite: true);
        return blobClient.Uri.ToString();
    }

    private static byte[] GeneratePolicyPdf(Policy policy)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Text("KEYSTONE INSURANCE")
                            .FontSize(20).Bold();
                        row.ConstantItem(150).Text("Commercial Property\nDeclaration Page")
                            .AlignRight();
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        col.Item().Text($"Policy Number: {policy.PolicyNumber}").Bold();
                        col.Item().Text($"Effective: {policy.EffectiveDate:d} to {policy.ExpirationDate:d}");
                        col.Item().Text($"Issue Date: {policy.IssueDate:d}");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("NAMED INSURED").Bold();
                        col.Item().Text(policy.Quote.Client.BusinessName);
                        col.Item().Text(policy.Quote.PropertyAddress);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("COVERAGE SUMMARY").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });
                            table.Cell().Text("Coverage Limit");
                            table.Cell().Text($"${policy.CoverageLimit:N0}");
                            table.Cell().Text("Deductible");
                            table.Cell().Text($"${policy.Deductible:N0}");
                            table.Cell().Text("Annual Premium");
                            table.Cell().Text($"${policy.AnnualPremium:N2}");
                            table.Cell().Text("Payment Plan");
                            table.Cell().Text(policy.PaymentPlan);
                        });

                        if (policy.BusinessInterruptionCoverage || policy.EquipmentBreakdownCoverage
                            || policy.FloodCoverage || policy.EarthquakeCoverage)
                        {
                            col.Item().PaddingVertical(10).LineHorizontal(1);
                            col.Item().Text("ADDITIONAL COVERAGES").Bold();

                            if (policy.BusinessInterruptionCoverage)
                                col.Item().Text($"Business Interruption: ${policy.BusinessInterruptionLimit:N0}");
                            if (policy.EquipmentBreakdownCoverage)
                                col.Item().Text("Equipment Breakdown: Included");
                            if (policy.FloodCoverage)
                                col.Item().Text($"Flood: ${policy.FloodLimit:N0}");
                            if (policy.EarthquakeCoverage)
                                col.Item().Text($"Earthquake: ${policy.EarthquakeLimit:N0}");
                        }
                    });
                });

                page.Footer().AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static byte[] GenerateEndorsementPdf(Endorsement endorsement)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Text("KEYSTONE INSURANCE")
                            .FontSize(20).Bold();
                        row.ConstantItem(150).Text("Endorsement")
                            .AlignRight();
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        col.Item().Text($"Endorsement Number: {endorsement.EndorsementNumber}").Bold();
                        col.Item().Text($"Policy Number: {endorsement.Policy.PolicyNumber}");
                        col.Item().Text($"Effective Date: {endorsement.EffectiveDate:d}");
                        col.Item().Text($"Type: {endorsement.EndorsementType}");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("INSURED").Bold();
                        col.Item().Text(endorsement.Policy.Quote.Client.BusinessName);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("ENDORSEMENT DETAILS").Bold();
                        if (!string.IsNullOrEmpty(endorsement.ChangeDescription))
                            col.Item().Text(endorsement.ChangeDescription);

                        if (endorsement.PremiumChange.HasValue)
                            col.Item().Text($"Premium Change: ${endorsement.PremiumChange:N2}");
                        if (endorsement.NewCoverageLimit.HasValue)
                            col.Item().Text($"New Coverage Limit: ${endorsement.NewCoverageLimit:N0}");
                        if (endorsement.NewDeductible.HasValue)
                            col.Item().Text($"New Deductible: ${endorsement.NewDeductible:N0}");
                    });
                });

                page.Footer().AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static byte[] GenerateQuotePdf(Quote quote)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Text("KEYSTONE INSURANCE")
                            .FontSize(20).Bold();
                        row.ConstantItem(150).Text("Quote Summary")
                            .AlignRight();
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        col.Item().Text($"Quote Number: {quote.QuoteNumber}").Bold();
                        col.Item().Text($"Date: {quote.CreatedDate:d}");
                        col.Item().Text($"Valid Until: {quote.ExpirationDate:d}");
                        col.Item().Text($"Status: {quote.Status}");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("APPLICANT").Bold();
                        col.Item().Text(quote.Client.BusinessName);
                        col.Item().Text($"{quote.PropertyAddress}, {quote.City}, {quote.StateCode} {quote.ZipCode}");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("PROPERTY DETAILS").Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });
                            table.Cell().Text("Construction Type");
                            table.Cell().Text(quote.ConstructionType);
                            table.Cell().Text("Occupancy Type");
                            table.Cell().Text(quote.OccupancyType);
                            table.Cell().Text("Year Built");
                            table.Cell().Text(quote.YearBuilt.ToString());
                            table.Cell().Text("Square Footage");
                            table.Cell().Text($"{quote.SquareFootage:N0}");
                            table.Cell().Text("Property Value");
                            table.Cell().Text($"${quote.PropertyValue:N0}");
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("PREMIUM SUMMARY").Bold();
                        if (quote.BasePremium.HasValue)
                            col.Item().Text($"Base Premium: ${quote.BasePremium:N2}");
                        if (quote.TotalPremium.HasValue)
                            col.Item().Text($"Total Premium: ${quote.TotalPremium:N2}").Bold();
                    });
                });

                page.Footer().AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static byte[] GenerateRenewalPdf(Policy policy)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(50);

                page.Header().Element(header =>
                {
                    header.Row(row =>
                    {
                        row.RelativeItem().Text("KEYSTONE INSURANCE")
                            .FontSize(20).Bold();
                        row.ConstantItem(150).Text("Renewal Notice")
                            .AlignRight();
                    });
                });

                page.Content().Element(content =>
                {
                    content.Column(col =>
                    {
                        col.Item().Text($"Policy Number: {policy.PolicyNumber}").Bold();
                        col.Item().Text($"Current Expiration: {policy.ExpirationDate:d}");

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("NAMED INSURED").Bold();
                        col.Item().Text(policy.Quote.Client.BusinessName);
                        col.Item().Text(policy.Quote.PropertyAddress);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().Text("RENEWAL OFFER").Bold();
                        col.Item().Text(
                            "Your current policy is approaching its expiration date. " +
                            "We are pleased to offer the following renewal terms:");

                        col.Item().PaddingVertical(5);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });
                            table.Cell().Text("Coverage Limit");
                            table.Cell().Text($"${policy.CoverageLimit:N0}");
                            table.Cell().Text("Deductible");
                            table.Cell().Text($"${policy.Deductible:N0}");
                            table.Cell().Text("Annual Premium");
                            table.Cell().Text($"${policy.AnnualPremium:N2}");
                            table.Cell().Text("Payment Plan");
                            table.Cell().Text(policy.PaymentPlan);
                        });

                        col.Item().PaddingVertical(10);
                        col.Item().Text(
                            "Please contact your agent to review and accept these renewal terms " +
                            "before the expiration date to ensure continuous coverage.");
                    });
                });

                page.Footer().AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        });

        return document.GeneratePdf();
    }
}
