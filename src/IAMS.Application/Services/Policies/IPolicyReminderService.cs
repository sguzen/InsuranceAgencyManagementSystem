using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Policies
{
    public interface IPolicyReminderService
    {
        Task<IEnumerable<Policy>> GetPoliciesForReminderAsync(DateTime reminderDate, int daysBefore = 30);
        Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(int daysBefore = 30);
        Task<IEnumerable<Policy>> GetOverduePoliciesAsync();
        Task SendPolicyReminderAsync(Policy policy, int daysBefore);
        Task SendBatchRemindersAsync(IEnumerable<Policy> policies, int daysBefore);
        Task ProcessExpiringPoliciesAsync();
    }
}