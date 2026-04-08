using System;
using System.Collections.Generic;

namespace KeystoneInsurance.Core.Integration
{
    /// <summary>
    /// Client for submitting regulatory reports to state insurance departments
    /// Handles NAIC reporting, state-specific filings, and compliance submissions
    /// </summary>
    public class RegulatoryReporter
    {
        public RegulatorySubmissionResponse SubmitQuarterlyReport(QuarterlyReportData reportData)
        {
            // Submit quarterly financial and statistical report
            var response = new RegulatorySubmissionResponse
            {
                SubmissionId = Guid.NewGuid().ToString(),
                ReportType = "Quarterly Financial",
                Quarter = reportData.Quarter,
                Year = reportData.Year,
                SubmissionDate = DateTime.Now,
                Status = "Accepted",
                ConfirmationNumber = GenerateConfirmationNumber()
            };
            
            // Validate report data
            var validationErrors = ValidateReportData(reportData);
            if (validationErrors.Count > 0)
            {
                response.Status = "Rejected";
                response.ValidationErrors = validationErrors;
            }
            
            return response;
        }
        
        public RegulatorySubmissionResponse SubmitRateFilingToState(string stateCode, RateFilingData filingData)
        {
            // Submit rate change filing to state insurance department
            var response = new RegulatorySubmissionResponse
            {
                SubmissionId = Guid.NewGuid().ToString(),
                ReportType = $"Rate Filing - {stateCode}",
                SubmissionDate = DateTime.Now,
                Status = "Pending Review",
                ConfirmationNumber = GenerateConfirmationNumber(),
                EstimatedApprovalDate = CalculateApprovalDate(stateCode)
            };
            
            return response;
        }
        
        public void SubmitPolicyTransaction(string stateCode, PolicyTransactionData transaction)
        {
            // Submit policy transaction for statistical reporting (NAIC)
            // This feeds into state-level market conduct and rate monitoring
            
            Console.WriteLine($"Submitting transaction to {stateCode} insurance department: " +
                            $"Policy {transaction.PolicyNumber}, Premium ${transaction.Premium:N2}");
        }
        
        private List<string> ValidateReportData(QuarterlyReportData reportData)
        {
            var errors = new List<string>();
            
            if (reportData.TotalPremiumWritten <= 0)
                errors.Add("Total premium written must be greater than zero");
            
            if (reportData.LossRatio > 2.0m)
                errors.Add("Loss ratio appears unusually high - please verify");
            
            if (reportData.ExpenseRatio > 0.50m)
                errors.Add("Expense ratio exceeds regulatory threshold");
                
            return errors;
        }
        
        private DateTime CalculateApprovalDate(string stateCode)
        {
            // Different states have different review periods
            int reviewDays = stateCode switch
            {
                "CA" => 90, // California Prop 103 - 90 day review
                "FL" => 45,
                "TX" => 30,
                "NY" => 60,
                _ => 45
            };
            
            return DateTime.Now.AddDays(reviewDays);
        }
        
        private string GenerateConfirmationNumber()
        {
            return $"NAIC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
    
    public class QuarterlyReportData
    {
        public int Quarter { get; set; }
        public int Year { get; set; }
        public decimal TotalPremiumWritten { get; set; }
        public decimal TotalPremiumEarned { get; set; }
        public decimal TotalLossesPaid { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal LossRatio { get; set; }
        public decimal ExpenseRatio { get; set; }
        public decimal CombinedRatio { get; set; }
        public int PolicyCount { get; set; }
    }
    
    public class RateFilingData
    {
        public string FilingType { get; set; }
        public decimal ProposedRateChange { get; set; }
        public string Justification { get; set; }
        public DateTime EffectiveDate { get; set; }
        public List<string> AffectedLines { get; set; }
    }
    
    public class PolicyTransactionData
    {
        public string PolicyNumber { get; set; }
        public string TransactionType { get; set; }
        public decimal Premium { get; set; }
        public DateTime TransactionDate { get; set; }
        public string StateCode { get; set; }
    }
    
    public class RegulatorySubmissionResponse
    {
        public string SubmissionId { get; set; }
        public string ReportType { get; set; }
        public int Quarter { get; set; }
        public int Year { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        public string ConfirmationNumber { get; set; }
        public DateTime? EstimatedApprovalDate { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}
