-- Keystone Insurance Database Schema
-- Commercial Property Insurance System
-- SQL Server 2012+

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'KeystoneInsurance')
BEGIN
    CREATE DATABASE KeystoneInsurance;
END
GO

USE KeystoneInsurance;
GO

-- Clients Table
CREATE TABLE Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    ClientNumber VARCHAR(20) UNIQUE NOT NULL,
    BusinessName NVARCHAR(200) NOT NULL,
    ContactFirstName NVARCHAR(100),
    ContactLastName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone VARCHAR(20),
    BusinessType NVARCHAR(100),
    YearsInBusiness INT,
    FederalTaxId VARCHAR(11),
    MailingAddress NVARCHAR(200),
    MailingCity NVARCHAR(100),
    MailingState VARCHAR(2),
    MailingZip VARCHAR(10),
    AccountCreatedDate DATETIME DEFAULT GETDATE(),
    AccountStatus VARCHAR(20) DEFAULT 'Active',
    CreditScore DECIMAL(5,2),
    RiskTier VARCHAR(20),
    TotalActivePolicies INT DEFAULT 0,
    TotalPremiumInForce DECIMAL(15,2) DEFAULT 0,
    ClaimsHistory INT DEFAULT 0,
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME
);

-- Quotes Table
CREATE TABLE Quotes (
    QuoteId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL FOREIGN KEY REFERENCES Clients(ClientId),
    QuoteNumber VARCHAR(30) UNIQUE NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ExpirationDate DATETIME NOT NULL,
    Status VARCHAR(20) NOT NULL, -- Draft, Pending, Approved, Declined, Expired, Bound
    
    -- Property Information
    PropertyAddress NVARCHAR(200) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    StateCode VARCHAR(2) NOT NULL,
    ZipCode VARCHAR(10) NOT NULL,
    CountyName NVARCHAR(100),
    PropertyValue DECIMAL(15,2) NOT NULL,
    ConstructionType VARCHAR(50) NOT NULL,
    OccupancyType VARCHAR(100) NOT NULL,
    YearBuilt INT NOT NULL,
    SquareFootage INT NOT NULL,
    NumberOfStories INT NOT NULL,
    SprinklersInstalled BIT DEFAULT 0,
    AlarmSystemInstalled BIT DEFAULT 0,
    RoofType VARCHAR(50),
    RoofAge INT,
    
    -- Coverage Details
    CoverageLimit DECIMAL(15,2) NOT NULL,
    Deductible DECIMAL(15,2) NOT NULL,
    BusinessInterruptionCoverage BIT DEFAULT 0,
    BusinessInterruptionLimit DECIMAL(15,2),
    EquipmentBreakdownCoverage BIT DEFAULT 0,
    FloodCoverage BIT DEFAULT 0,
    EarthquakeCoverage BIT DEFAULT 0,
    
    -- Loss History
    PriorClaimsCount INT DEFAULT 0,
    PriorClaimsTotalAmount DECIMAL(15,2) DEFAULT 0,
    
    -- Premium Calculation Results
    BasePremium DECIMAL(10,2),
    TotalPremium DECIMAL(10,2),
    PremiumCalculationDetails NVARCHAR(MAX),
    
    -- Underwriting
    UnderwriterId INT,
    UnderwritingNotes NVARCHAR(MAX),
    
    -- Audit
    CreatedBy NVARCHAR(100),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME
);

CREATE INDEX IX_Quotes_ClientId ON Quotes(ClientId);
CREATE INDEX IX_Quotes_Status ON Quotes(Status);
CREATE INDEX IX_Quotes_StateCode ON Quotes(StateCode);

-- Underwriting Decisions Table
CREATE TABLE UnderwritingDecisions (
    UWId INT IDENTITY(1,1) PRIMARY KEY,
    QuoteId INT NOT NULL FOREIGN KEY REFERENCES Quotes(QuoteId),
    UnderwriterId INT NOT NULL,
    DecisionDate DATETIME DEFAULT GETDATE(),
    Decision VARCHAR(20) NOT NULL, -- Approved, Declined, ReferToSenior, RequestMoreInfo
    RiskScore DECIMAL(5,2) NOT NULL,
    
    -- Risk Assessment
    CatastropheZoneRating VARCHAR(20),
    ConstructionRating VARCHAR(20),
    OccupancyRating VARCHAR(20),
    ProtectionRating VARCHAR(20),
    LossHistoryRating VARCHAR(20),
    
    -- Catastrophe Exposure
    HighCatExposure BIT DEFAULT 0,
    CatastrophePML DECIMAL(15,2), -- Probable Maximum Loss
    
    -- Conditions and Stipulations
    ApprovalConditions NVARCHAR(MAX),
    DeclineReason NVARCHAR(500),
    AdditionalInformationRequired NVARCHAR(MAX),
    
    -- Referral
    ReferredToSeniorUnderwriter BIT DEFAULT 0,
    SeniorUnderwriterId INT,
    ReferralReason NVARCHAR(500),
    
    UnderwritingNotes NVARCHAR(MAX),
    
    -- Audit
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE INDEX IX_UW_QuoteId ON UnderwritingDecisions(QuoteId);
CREATE INDEX IX_UW_Decision ON UnderwritingDecisions(Decision);

-- Policies Table
CREATE TABLE Policies (
    PolicyId INT IDENTITY(1,1) PRIMARY KEY,
    QuoteId INT NOT NULL FOREIGN KEY REFERENCES Quotes(QuoteId),
    PolicyNumber VARCHAR(30) UNIQUE NOT NULL,
    EffectiveDate DATE NOT NULL,
    ExpirationDate DATE NOT NULL,
    IssueDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(30) NOT NULL, -- Active, Cancelled, Expired, PendingCancellation, Renewed
    
    -- Premium and Payment
    AnnualPremium DECIMAL(10,2) NOT NULL,
    PaymentPlan VARCHAR(20) NOT NULL, -- Annual, SemiAnnual, Quarterly, Monthly
    InstallmentAmount DECIMAL(10,2),
    NextPaymentDue DATE,
    
    -- Coverage
    CoverageLimit DECIMAL(15,2) NOT NULL,
    Deductible DECIMAL(15,2) NOT NULL,
    CoverageType VARCHAR(50) DEFAULT 'Commercial Property',
    
    -- Additional Coverages
    BusinessInterruptionCoverage BIT DEFAULT 0,
    BusinessInterruptionLimit DECIMAL(15,2),
    EquipmentBreakdownCoverage BIT DEFAULT 0,
    FloodCoverage BIT DEFAULT 0,
    FloodLimit DECIMAL(15,2),
    EarthquakeCoverage BIT DEFAULT 0,
    EarthquakeLimit DECIMAL(15,2),
    
    -- Reinsurance
    ReinsuranceCeded BIT DEFAULT 0,
    CededPremium DECIMAL(10,2),
    ReinsuranceTreatyId VARCHAR(50),
    
    -- Cancellation
    CancellationDate DATE,
    CancellationReason NVARCHAR(500),
    ReturnPremium DECIMAL(10,2),
    
    -- Documents
    PolicyDocumentPath NVARCHAR(500),
    DocumentGeneratedDate DATETIME,
    
    -- Audit
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME
);

CREATE INDEX IX_Policies_QuoteId ON Policies(QuoteId);
CREATE INDEX IX_Policies_PolicyNumber ON Policies(PolicyNumber);
CREATE INDEX IX_Policies_Status ON Policies(Status);
CREATE INDEX IX_Policies_EffectiveDate ON Policies(EffectiveDate);

-- Endorsements Table
CREATE TABLE Endorsements (
    EndorsementId INT IDENTITY(1,1) PRIMARY KEY,
    PolicyId INT NOT NULL FOREIGN KEY REFERENCES Policies(PolicyId),
    EndorsementNumber VARCHAR(50) UNIQUE NOT NULL,
    EffectiveDate DATE NOT NULL,
    RequestDate DATETIME DEFAULT GETDATE(),
    EndorsementType VARCHAR(30) NOT NULL, -- CoverageChange, PremiumAdjustment, NameChange, AddressChange, Cancellation
    Status VARCHAR(20) DEFAULT 'Pending',
    
    -- Changes
    ChangeDescription NVARCHAR(MAX),
    PremiumChange DECIMAL(10,2),
    NewCoverageLimit DECIMAL(15,2),
    NewDeductible DECIMAL(15,2),
    
    -- Approval
    ApprovedBy INT,
    ApprovalDate DATETIME,
    
    -- Documents
    EndorsementDocumentPath NVARCHAR(500),
    
    -- Audit
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE INDEX IX_Endorsements_PolicyId ON Endorsements(PolicyId);

-- Rate Factors Table
CREATE TABLE RateFactors (
    FactorId INT IDENTITY(1,1) PRIMARY KEY,
    FactorType VARCHAR(50) NOT NULL,
    FactorCode VARCHAR(20) NOT NULL,
    Description NVARCHAR(200),
    FactorValue DECIMAL(8,4) NOT NULL,
    EffectiveDate DATE NOT NULL,
    ExpirationDate DATE NOT NULL,
    StateCode VARCHAR(2),
    TerritoryCode VARCHAR(10),
    IsActive BIT DEFAULT 1,
    
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE INDEX IX_RateFactors_Type ON RateFactors(FactorType, StateCode);
CREATE INDEX IX_RateFactors_Effective ON RateFactors(EffectiveDate, ExpirationDate);

-- Coverage Options Table
CREATE TABLE CoverageOptions (
    CoverageId INT IDENTITY(1,1) PRIMARY KEY,
    CoverageType VARCHAR(50) NOT NULL,
    CoverageCode VARCHAR(20) NOT NULL,
    Description NVARCHAR(500),
    IsOptional BIT DEFAULT 1,
    BaseRate DECIMAL(8,6),
    RatingBasis VARCHAR(30),
    IsActive BIT DEFAULT 1
);

-- Audit Log Table
CREATE TABLE AuditLog (
    AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName VARCHAR(50) NOT NULL,
    RecordId INT NOT NULL,
    Action VARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    FieldName VARCHAR(50),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    ChangedBy NVARCHAR(100),
    ChangedDate DATETIME DEFAULT GETDATE()
);

CREATE INDEX IX_Audit_Table ON AuditLog(TableName, RecordId);
CREATE INDEX IX_Audit_Date ON AuditLog(ChangedDate);

-- Create sample stored procedures stubs (200+ in real system)
GO

PRINT 'Database schema created successfully';
GO
