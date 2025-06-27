namespace IAMS.Application.DTOs.Policy
{
    public class CreatePolicyDto
    {
        public string PolicyNumber { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public int PolicyTypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public string? Notes { get; set; }
        public string Currency { get; set; } = "TRY";
    }
}