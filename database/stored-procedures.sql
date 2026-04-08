-- Sample Stored Procedures for Keystone Insurance
-- This represents a subset of the 200+ stored procedures in the system
USE KeystoneInsurance;
GO

-- Procedure: Get Quote Details with Premium Breakdown
CREATE PROCEDURE usp_GetQuoteDetails
    @QuoteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        q.*,
        c.BusinessName,
        c.ContactFirstName,
        c.ContactLastName,
        c.Email,
        uw.Decision AS UnderwritingDecision,
        uw.RiskScore,
        uw.ApprovalConditions
    FROM Quotes q
    INNER JOIN Clients c ON q.ClientId = c.ClientId
    LEFT JOIN UnderwritingDecisions uw ON q.QuoteId = uw.QuoteId
    WHERE q.QuoteId = @QuoteId;
END
GO

-- Procedure: Calculate Premium (calls business logic)
CREATE PROCEDURE usp_CalculatePremium
    @PropertyValue DECIMAL(15,2),
    @ConstructionType VARCHAR(50),
    @StateCode VARCHAR(2),
    @CoverageLimit DECIMAL(15,2),
    @Deductible DECIMAL(15,2),
    @CalculatedPremium DECIMAL(10,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Simplified premium calculation (actual calculation in QuotingEngine service)
    DECLARE @BaseRate DECIMAL(10,2);
    DECLARE @ConstructionFactor DECIMAL(5,4);
    DECLARE @StateFactor DECIMAL(5,4);
    
    -- Get base rate
    SELECT @ConstructionFactor = FactorValue 
    FROM RateFactors 
    WHERE FactorType = 'Construction' 
        AND FactorCode = @ConstructionType
        AND GETDATE() BETWEEN EffectiveDate AND ExpirationDate;
    
    -- Calculate
    SET @BaseRate = 500.00;
    SET @CalculatedPremium = @PropertyValue * 0.002 * ISNULL(@ConstructionFactor, 1.0);
    
END
GO

-- Procedure: Search Quotes
CREATE PROCEDURE usp_SearchQuotes
    @ClientId INT = NULL,
    @StateCode VARCHAR(2) = NULL,
    @Status VARCHAR(20) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        q.QuoteId,
        q.QuoteNumber,
        q.CreatedDate,
        q.ExpirationDate,
        q.Status,
        q.PropertyAddress,
        q.City,
        q.StateCode,
        q.TotalPremium,
        c.BusinessName
    FROM Quotes q
    INNER JOIN Clients c ON q.ClientId = c.ClientId
    WHERE 
        (@ClientId IS NULL OR q.ClientId = @ClientId)
        AND (@StateCode IS NULL OR q.StateCode = @StateCode)
        AND (@Status IS NULL OR q.Status = @Status)
        AND (@FromDate IS NULL OR q.CreatedDate >= @FromDate)
        AND (@ToDate IS NULL OR q.CreatedDate <= @ToDate)
    ORDER BY q.CreatedDate DESC;
END
GO

-- Procedure: Get Policies Expiring Soon
CREATE PROCEDURE usp_GetExpiringPolicies
    @DaysAhead INT = 60
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.PolicyId,
        p.PolicyNumber,
        p.ExpirationDate,
        p.AnnualPremium,
        c.BusinessName,
        c.Email,
        q.PropertyAddress,
        q.City,
        q.StateCode
    FROM Policies p
    INNER JOIN Quotes q ON p.QuoteId = q.QuoteId
    INNER JOIN Clients c ON q.ClientId = c.ClientId
    WHERE 
        p.Status = 'Active'
        AND p.ExpirationDate BETWEEN GETDATE() AND DATEADD(DAY, @DaysAhead, GETDATE())
    ORDER BY p.ExpirationDate;
END
GO

-- Procedure: Update Policy Status
CREATE PROCEDURE usp_UpdatePolicyStatus
    @PolicyId INT,
    @NewStatus VARCHAR(30),
    @ModifiedBy NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Policies
    SET Status = @NewStatus,
        ModifiedBy = @ModifiedBy,
        ModifiedDate = GETDATE()
    WHERE PolicyId = @PolicyId;
    
    -- Log to audit table
    INSERT INTO AuditLog (TableName, RecordId, Action, FieldName, NewValue, ChangedBy)
    VALUES ('Policies', @PolicyId, 'UPDATE', 'Status', @NewStatus, @ModifiedBy);
END
GO

-- Procedure: Get Premium Summary by State
CREATE PROCEDURE usp_GetPremiumSummaryByState
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        q.StateCode,
        COUNT(*) AS QuoteCount,
        SUM(q.TotalPremium) AS TotalPremium,
        AVG(q.TotalPremium) AS AveragePremium,
        MIN(q.TotalPremium) AS MinPremium,
        MAX(q.TotalPremium) AS MaxPremium
    FROM Quotes q
    WHERE YEAR(q.CreatedDate) = @Year
        AND q.Status IN ('Approved', 'Bound')
    GROUP BY q.StateCode
    ORDER BY TotalPremium DESC;
END
GO

PRINT 'Stored procedures created successfully';
GO
