using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Infrastructure.Interfaces
{
    public interface IPolicyService
    {
        Task ProcessExpiringPoliciesAsync();
        Task<List<ExpiringPolicyDto>> GetExpiringPoliciesAsync(int daysAhead = 30);
        Task SendPolicyReminderAsync(int policyId);
        Task<bool> RenewPolicyAsync(int policyId, RenewPolicyRequest request);
        Task<PolicyStatusDto> GetPolicyStatusAsync(int policyId);
    }

    public class ExpiringPolicyDto
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public decimal PremiumAmount { get; set; }
        public string InsuranceCompany { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public int DaysToExpiry { get; set; }
        public bool ReminderSent { get; set; }
    }

    public class RenewPolicyRequest
    {
        public decimal NewPremium { get; set; }
        public DateTime NewExpiryDate { get; set; }
        public string? Notes { get; set; }
    }

    public class PolicyStatusDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StatusDate { get; set; }
        public string? Notes { get; set; }
    }

}
