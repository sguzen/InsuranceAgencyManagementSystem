namespace IAMS.Application.DTOs.Commission
{
    public class CommissionSummaryDto
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public string PolicyTypeName { get; set; } = string.Empty;
        public decimal PremiumAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime PolicyStartDate { get; set; }
        public DateTime PolicyEndDate { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
        public string PolicyStatus { get; set; } = string.Empty;
    }
}
