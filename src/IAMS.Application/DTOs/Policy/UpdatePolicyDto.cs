using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.DTOs.Policy
{
    public class UpdatePolicyDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public PolicyStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
