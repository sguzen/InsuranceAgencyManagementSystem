using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Enums;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for policy analytics, counts, and revenue calculations
    /// </summary>
    public class PolicyAnalyticsService : IPolicyAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PolicyAnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Delegate to repository
        private dynamic PolicyRepo => (dynamic)_unitOfWork.Policies;

        public Task<int> GetPolicyCountAsync()
            => PolicyRepo.GetPolicyCountAsync();

        public Task<int> GetActivePolicyCountAsync()
            => PolicyRepo.GetActivePolicyCountAsync();

        public Task<int> GetExpiringPolicyCountAsync(int daysAhead)
            => PolicyRepo.GetExpiringPolicyCountAsync(daysAhead);

        public Task<int> GetExpiredPolicyCountAsync()
            => PolicyRepo.GetExpiredPolicyCountAsync();

        public Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync()
            => PolicyRepo.GetPolicyCountByStatusAsync();

        public Task<decimal> GetMonthlyRevenueAsync(DateTime? month = null)
            => PolicyRepo.GetMonthlyRevenueAsync(month);

        public Task<Dictionary<string, decimal>> GetMonthlyRevenueByCurrencyAsync(DateTime? month = null)
            => PolicyRepo.GetMonthlyRevenueByCurrencyAsync(month);

        public Task<decimal> GetYearlyRevenueAsync(int? year = null)
            => PolicyRepo.GetYearlyRevenueAsync(year);

        public Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int months = 12)
            => PolicyRepo.GetRevenueByMonthAsync(months);

        public Task<decimal> GetTotalPremiumByCustomerAsync(int customerId)
            => PolicyRepo.GetTotalPremiumByCustomerAsync(customerId);

        public Task<Dictionary<string, object>> GetPolicyAnalyticsAsync()
            => PolicyRepo.GetPolicyAnalyticsAsync();
    }
}
