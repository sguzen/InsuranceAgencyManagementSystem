using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for policy analytics, counts, and revenue calculations
    /// </summary>
    public interface IPolicyAnalyticsService
    {
        // Count queries
        Task<int> GetPolicyCountAsync();
        Task<int> GetActivePolicyCountAsync();
        Task<int> GetExpiringPolicyCountAsync(int daysAhead);
        Task<int> GetExpiredPolicyCountAsync();
        Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync();

        // Revenue queries
        Task<decimal> GetMonthlyRevenueAsync(DateTime? month = null);
        Task<Dictionary<string, decimal>> GetMonthlyRevenueByCurrencyAsync(DateTime? month = null);
        Task<decimal> GetYearlyRevenueAsync(int? year = null);
        Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int months = 12);
        Task<decimal> GetTotalPremiumByCustomerAsync(int customerId);

        // Analytics dashboard
        Task<Dictionary<string, object>> GetPolicyAnalyticsAsync();
    }
}
