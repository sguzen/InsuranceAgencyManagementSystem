using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for policy analytics, counts, and revenue calculations
    /// </summary>
    public class PolicyAnalyticsService : IPolicyAnalyticsService
    {
        private readonly IPolicyRepository _policyRepository;

        public PolicyAnalyticsService(IUnitOfWork unitOfWork)
        {
            _policyRepository = unitOfWork.Policies;
        }

        public async Task<int> GetPolicyCountAsync()
        {
            return await _policyRepository.AsQueryable().CountAsync();
        }

        public async Task<int> GetActivePolicyCountAsync()
        {
            return await _policyRepository.AsQueryable()
                .CountAsync(p => p.Status == PolicyStatus.Active);
        }

        public async Task<int> GetExpiringPolicyCountAsync(int daysAhead)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _policyRepository.AsQueryable()
                .CountAsync(p => p.Status == PolicyStatus.Active &&
                               p.EndDate <= cutoffDate &&
                               p.EndDate >= DateTime.Now);
        }

        public async Task<int> GetExpiredPolicyCountAsync()
        {
            return await _policyRepository.AsQueryable()
                .CountAsync(p => p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now);
        }

        public async Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync()
        {
            return await _policyRepository.AsQueryable()
                .GroupBy(p => p.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<decimal> GetMonthlyRevenueAsync(DateTime? month = null)
        {
            var targetMonth = month ?? DateTime.Now;
            var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _policyRepository.AsQueryable()
                .Where(p => p.Status == PolicyStatus.Active &&
                           p.StartDate <= endOfMonth &&
                           p.EndDate >= startOfMonth)
                .SumAsync(p => p.Premium);
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyRevenueByCurrencyAsync(DateTime? month = null)
        {
            var targetMonth = month ?? DateTime.Now;
            var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _policyRepository.AsQueryable()
                .Where(p => p.Status == PolicyStatus.Active &&
                           p.StartDate <= endOfMonth &&
                           p.EndDate >= startOfMonth)
                .Include(p => p.Currency)
                .GroupBy(p => p.Currency.Code)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(p => p.Premium));
        }

        public async Task<decimal> GetYearlyRevenueAsync(int? year = null)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var startOfYear = new DateTime(targetYear, 1, 1);
            var endOfYear = new DateTime(targetYear, 12, 31);

            return await _policyRepository.AsQueryable()
                .Where(p => p.Status == PolicyStatus.Active &&
                           p.StartDate <= endOfYear &&
                           p.EndDate >= startOfYear)
                .SumAsync(p => p.Premium);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int months = 12)
        {
            var result = new Dictionary<string, decimal>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < months; i++)
            {
                var monthDate = currentDate.AddMonths(-i);
                var revenue = await GetMonthlyRevenueAsync(monthDate);
                result[monthDate.ToString("yyyy-MM")] = revenue;
            }

            return result;
        }

        public async Task<decimal> GetTotalPremiumByCustomerAsync(int customerId)
        {
            return await _policyRepository.AsQueryable()
                .Where(p => p.CustomerId == customerId)
                .SumAsync(p => p.Premium);
        }

        public async Task<Dictionary<string, object>> GetPolicyAnalyticsAsync()
        {
            var policies = _policyRepository.AsQueryable();

            return new Dictionary<string, object>
            {
                { "TotalPolicies", await policies.CountAsync() },
                { "ActivePolicies", await policies.CountAsync(p => p.Status == PolicyStatus.Active) },
                { "ExpiredPolicies", await policies.CountAsync(p => p.Status == PolicyStatus.Expired) },
                { "CancelledPolicies", await policies.CountAsync(p => p.Status == PolicyStatus.Cancelled) },
                { "MonthlyRevenue", await GetMonthlyRevenueAsync() },
                { "YearlyRevenue", await GetYearlyRevenueAsync() }
            };
        }
    }
}
