using IAMS.Domain.Entities;

namespace IAMS.Domain.Services
{
    public interface IPolicyReminderService
    {
        Task<IEnumerable<Policy>> GetPoliciesForReminderAsync(DateTime reminderDate, int daysBefore = 30);
        Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(int tenantId, int daysBefore = 30);
        Task<IEnumerable<Policy>> GetOverduePoliciesAsync(int tenantId);
        Task SendPolicyReminderAsync(Policy policy, int daysBefore);
        Task SendBatchRemindersAsync(IEnumerable<Policy> policies, int daysBefore);
    }
}