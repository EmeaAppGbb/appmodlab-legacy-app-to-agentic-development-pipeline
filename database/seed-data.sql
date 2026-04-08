-- Seed Data for Keystone Insurance System
USE KeystoneInsurance;
GO

-- Insert sample clients
INSERT INTO Clients (ClientNumber, BusinessName, ContactFirstName, ContactLastName, Email, Phone, 
    BusinessType, YearsInBusiness, MailingAddress, MailingCity, MailingState, MailingZip, RiskTier)
VALUES
('C0001', 'Acme Manufacturing Corp', 'John', 'Smith', 'john.smith@acme.com', '555-0101', 
    'Manufacturing', 15, '123 Industrial Pkwy', 'Chicago', 'IL', '60601', 'Standard'),
('C0002', 'Downtown Office Plaza LLC', 'Sarah', 'Johnson', 'sarah.j@downtownplaza.com', '555-0102',
    'Real Estate', 8, '456 Main Street', 'New York', 'NY', '10001', 'Preferred'),
('C0003', 'Coastal Retail Ventures', 'Michael', 'Davis', 'mdavis@coastalretail.com', '555-0103',
    'Retail', 22, '789 Beach Blvd', 'Miami', 'FL', '33139', 'Standard'),
('C0004', 'Tech Campus Holdings', 'Jennifer', 'Wilson', 'jwilson@techcampus.com', '555-0104',
    'Technology', 5, '321 Innovation Dr', 'San Francisco', 'CA', '94102', 'Preferred'),
('C0005', 'Heritage Restaurant Group', 'David', 'Brown', 'dbrown@heritagerest.com', '555-0105',
    'Restaurant', 12, '654 Culinary Way', 'Austin', 'TX', '78701', 'Standard');

-- Insert rate factors for different states and construction types
INSERT INTO RateFactors (FactorType, FactorCode, Description, FactorValue, EffectiveDate, ExpirationDate, StateCode)
VALUES
('Construction', 'FRAME', 'Frame Construction', 1.45, '2024-01-01', '2024-12-31', NULL),
('Construction', 'JMASON', 'Joisted Masonry', 1.25, '2024-01-01', '2024-12-31', NULL),
('Construction', 'NONCOMB', 'Non-Combustible', 1.10, '2024-01-01', '2024-12-31', NULL),
('Construction', 'MASONN', 'Masonry Non-Combustible', 0.95, '2024-01-01', '2024-12-31', NULL),
('Construction', 'MODFR', 'Modified Fire Resistive', 0.80, '2024-01-01', '2024-12-31', NULL),
('Construction', 'FR', 'Fire Resistive', 0.65, '2024-01-01', '2024-12-31', NULL),

('Territory', 'CA-URBAN', 'California Urban', 1.25, '2024-01-01', '2024-12-31', 'CA'),
('Territory', 'FL-COAST', 'Florida Coastal', 1.45, '2024-01-01', '2024-12-31', 'FL'),
('Territory', 'TX-METRO', 'Texas Metropolitan', 1.10, '2024-01-01', '2024-12-31', 'TX'),
('Territory', 'NY-URBAN', 'New York Urban', 1.20, '2024-01-01', '2024-12-31', 'NY'),
('Territory', 'IL-METRO', 'Illinois Metropolitan', 1.05, '2024-01-01', '2024-12-31', 'IL'),

('Occupancy', 'OFFICE', 'Office Building', 0.85, '2024-01-01', '2024-12-31', NULL),
('Occupancy', 'RETAIL', 'Retail Store', 1.00, '2024-01-01', '2024-12-31', NULL),
('Occupancy', 'RESTAURANT', 'Restaurant', 1.35, '2024-01-01', '2024-12-31', NULL),
('Occupancy', 'MANUFL', 'Light Manufacturing', 1.15, '2024-01-01', '2024-12-31', NULL),
('Occupancy', 'MANUFH', 'Heavy Manufacturing', 1.50, '2024-01-01', '2024-12-31', NULL),
('Occupancy', 'WAREHOUSE', 'Warehouse', 0.95, '2024-01-01', '2024-12-31', NULL);

-- Insert coverage options
INSERT INTO CoverageOptions (CoverageType, CoverageCode, Description, IsOptional, BaseRate, RatingBasis)
VALUES
('Property', 'BUILDING', 'Building Coverage', 0, 0.0020, 'Per $100 of value'),
('Business Interruption', 'BI', 'Business Interruption Coverage', 1, 0.0035, 'Per $100 of limit'),
('Equipment Breakdown', 'EB', 'Equipment Breakdown Coverage', 1, 0.0008, 'Per $100 of property value'),
('Flood', 'FLOOD', 'Flood Coverage', 1, 0.0025, 'Per $100 of limit'),
('Earthquake', 'EQ', 'Earthquake Coverage', 1, 0.0045, 'Per $100 of limit'),
('Ordinance or Law', 'OL', 'Ordinance or Law Coverage', 1, 0.0012, 'Per $100 of limit');

-- Insert sample quote
INSERT INTO Quotes (ClientId, QuoteNumber, CreatedDate, ExpirationDate, Status,
    PropertyAddress, City, StateCode, ZipCode, PropertyValue, ConstructionType, OccupancyType,
    YearBuilt, SquareFootage, NumberOfStories, SprinklersInstalled, AlarmSystemInstalled,
    RoofType, RoofAge, CoverageLimit, Deductible, BusinessInterruptionCoverage, BusinessInterruptionLimit,
    PriorClaimsCount, PriorClaimsTotalAmount, BasePremium, TotalPremium)
VALUES
(1, 'Q20240101-ABC12345', '2024-01-15', '2024-02-14', 'Approved',
    '123 Industrial Pkwy', 'Chicago', 'IL', '60601', 1500000, 'Non-Combustible', 'Manufacturing-Light',
    2010, 25000, 2, 1, 1, 'TPO/EPDM', 5, 1500000, 25000, 1, 500000, 0, 0, 8250.00, 9875.50);

-- Insert corresponding underwriting decision
INSERT INTO UnderwritingDecisions (QuoteId, UnderwriterId, DecisionDate, Decision, RiskScore,
    CatastropheZoneRating, ConstructionRating, OccupancyRating, ProtectionRating, LossHistoryRating,
    HighCatExposure, CatastrophePML, ApprovalConditions)
VALUES
(1, 1, '2024-01-16', 'Approved', 45.5, 'Moderate', 'Good', 'Average Risk', 'Superior', 'Loss Free',
    0, 375000, 'Standard terms apply');

PRINT 'Seed data inserted successfully';
GO
