namespace KeystoneInsurance.Modern.Domain.Entities;

public class Property
{
    public int PropertyId { get; set; }
    public int QuoteId { get; set; }

    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string StateCode { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string? CountyName { get; set; }

    public decimal PropertyValue { get; set; }
    public string ConstructionType { get; set; } = null!;
    public string OccupancyType { get; set; } = null!;
    public int YearBuilt { get; set; }
    public int SquareFootage { get; set; }
    public int NumberOfStories { get; set; }

    // Protection
    public bool SprinklersInstalled { get; set; }
    public bool AlarmSystemInstalled { get; set; }

    // Roof
    public string? RoofType { get; set; }
    public int RoofAge { get; set; }
}
