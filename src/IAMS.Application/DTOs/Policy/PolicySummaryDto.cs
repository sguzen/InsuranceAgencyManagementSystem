using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    public class PolicySummaryDto
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string InsuranceCompanyName { get; set; } = string.Empty;
        public string PolicyTypeName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public PolicyStatus Status { get; set; }
        public string Currency { get; set; } = "TRY";

        // Calculated properties
        public bool IsExpired => EndDate < DateTime.Today;
        public int DaysUntilExpiry => (EndDate - DateTime.Today).Days;
        public bool IsExpiringSoon => DaysUntilExpiry <= 30 && DaysUntilExpiry >= 0;
        public bool CanRenew => Status == PolicyStatus.Expired ||
                                (Status == PolicyStatus.Active && EndDate <= DateTime.Today.AddDays(60));
    }
}