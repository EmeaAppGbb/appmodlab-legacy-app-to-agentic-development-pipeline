using System;

namespace KeystoneInsurance.Core.Domain.Entities
{
    public class Property
    {
        public int PropertyId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
        public string CountyName { get; set; }
        
        // Physical Characteristics
        public decimal PropertyValue { get; set; }
        public string ConstructionType { get; set; }
        public int YearBuilt { get; set; }
        public int SquareFootage { get; set; }
        public int NumberOfStories { get; set; }
        public string RoofType { get; set; }
        public int RoofAge { get; set; }
        public string FoundationType { get; set; }
        
        // Protection
        public bool SprinklersInstalled { get; set; }
        public string SprinklerType { get; set; }
        public bool AlarmSystemInstalled { get; set; }
        public string AlarmSystemType { get; set; }
        public bool CentralStationMonitored { get; set; }
        public int ProtectionClassCode { get; set; }
        public decimal DistanceToFireStation { get; set; }
        public decimal DistanceToHydrant { get; set; }
        
        // Occupancy
        public string OccupancyType { get; set; }
        public string BusinessOperations { get; set; }
        public int NumberOfEmployees { get; set; }
        public bool HazardousMaterialsStored { get; set; }
        
        // Catastrophe Exposure
        public string CatastropheZone { get; set; }
        public int WindZone { get; set; }
        public bool FloodZone { get; set; }
        public string FloodZoneDesignation { get; set; }
        public int EarthquakeZone { get; set; }
        public bool CoastalExposure { get; set; }
        
        // Environmental
        public bool EnvironmentalHazardsIdentified { get; set; }
        public string EnvironmentalAssessmentDate { get; set; }
    }
}
