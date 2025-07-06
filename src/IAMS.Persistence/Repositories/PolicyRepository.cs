using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class PolicyRepository : Repository<Policy>, IPolicyRepository
    {
        public PolicyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Customer)
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByCompanyIdAsync(int companyId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PolicyType)
                .Where(p => p.InsuranceCompanyId == companyId && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPoliciesByStatusAsync(PolicyStatus status)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.Status == status && !p.IsDeleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(DateTime date)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.EndDate <= date && p.Status == PolicyStatus.Active && !p.IsDeleted)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<PagedResult<Policy>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => !p.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(p =>
                    p.PolicyNumber.ToLower().Contains(search) ||
                    p.Customer.FirstName.ToLower().Contains(search) ||
                    p.Customer.LastName.ToLower().Contains(search) ||
                    p.InsuranceCompany.Name.ToLower().Contains(search) ||
                    p.PolicyType.Name.ToLower().Contains(search));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var policies = await query
                .OrderByDescending(p => p.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Policy>
            {
                Items = policies,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.PolicyPayments)
                .Include(p => p.PolicyClaims)
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }

        public async Task<decimal> GetTotalPremiumByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && !p.IsDeleted && p.IsActive)
                .SumAsync(p => p.PremiumAmount);
        }

        public async Task<decimal> GetTotalCommissionByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => p.CustomerId == customerId && !p.IsDeleted && p.IsActive)
                .SumAsync(p => p.CommissionAmount);
        }

        public async Task<int> GetPolicyCountAsync(int tenantId)
        {
            try
            {
                return await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                    .CountAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting policy count for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<int> GetExpiringPoliciesCountAsync(int tenantId, int daysAhead = 30)
        {
            try
            {
                var cutoffDate = DateTime.Today.AddDays(daysAhead);

                return await _dbSet
                    .Where(p => !p.IsDeleted &&
                               p.TenantId == tenantId &&
                               p.Status == PolicyStatus.Active &&
                               p.EndDate <= cutoffDate &&
                               p.EndDate >= DateTime.Today)
                    .CountAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting expiring policies count for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int tenantId)
        {
            try
            {
                var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                return await _dbSet
                    .Where(p => !p.IsDeleted &&
                               p.TenantId == tenantId &&
                               p.Status == PolicyStatus.Active &&
                               p.StartDate >= startOfMonth &&
                               p.StartDate <= endOfMonth)
                    .SumAsync(p => p.PremiumAmount);
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting monthly revenue for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<PolicyStatisticsDto> GetPolicyStatisticsAsync(int tenantId)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var startOfYear = new DateTime(now.Year, 1, 1);
                var lastMonth = startOfMonth.AddMonths(-1);
                var endOfLastMonth = startOfMonth.AddDays(-1);

                var totalPolicies = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                    .CountAsync();

                var activePolicies = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.Status == PolicyStatus.Active)
                    .CountAsync();

                var expiredPolicies = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.Status == PolicyStatus.Expired)
                    .CountAsync();

                var cancelledPolicies = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.Status == PolicyStatus.Cancelled)
                    .CountAsync();

                var expiringPolicies = await GetExpiringPoliciesCountAsync(tenantId, 30);

                var totalPremiumAmount = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.Status == PolicyStatus.Active)
                    .SumAsync(p => p.PremiumAmount);

                var averagePremiumAmount = activePolicies > 0 ? totalPremiumAmount / activePolicies : 0;

                var monthlyRevenue = await GetMonthlyRevenueAsync(tenantId);

                var yearlyRevenue = await _dbSet
                    .Where(p => !p.IsDeleted &&
                               p.TenantId == tenantId &&
                               p.Status == PolicyStatus.Active &&
                               p.StartDate >= startOfYear)
                    .SumAsync(p => p.PremiumAmount);

                var lastMonthRevenue = await _dbSet
                    .Where(p => !p.IsDeleted &&
                               p.TenantId == tenantId &&
                               p.Status == PolicyStatus.Active &&
                               p.StartDate >= lastMonth &&
                               p.StartDate <= endOfLastMonth)
                    .SumAsync(p => p.PremiumAmount);

                var revenueGrowthPercentage = lastMonthRevenue > 0
                    ? ((monthlyRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                    : monthlyRevenue > 0 ? 100 : 0;

                var newPoliciesThisMonth = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.CreatedOn >= startOfMonth)
                    .CountAsync();

                var newPoliciesThisWeek = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.CreatedOn >= startOfWeek)
                    .CountAsync();

                var policiesByStatus = await GetPoliciesByStatusAsync(tenantId);

                var policiesByType = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                    .Include(p => p.PolicyType)
                    .GroupBy(p => p.PolicyType.Name)
                    .Select(g => new { TypeName = g.Key, Count = g.Count() })
                    .ToListAsync();

                var premiumByInsuranceCompany = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.Status == PolicyStatus.Active)
                    .Include(p => p.InsuranceCompany)
                    .GroupBy(p => p.InsuranceCompany.Name)
                    .Select(g => new { CompanyName = g.Key, TotalPremium = g.Sum(p => p.PremiumAmount) })
                    .ToListAsync();

                return new PolicyStatisticsDto
                {
                    TotalPolicies = totalPolicies,
                    ActivePolicies = activePolicies,
                    ExpiredPolicies = expiredPolicies,
                    ExpiringPolicies = expiringPolicies,
                    CancelledPolicies = cancelledPolicies,
                    TotalPremiumAmount = totalPremiumAmount,
                    AveragePremiumAmount = averagePremiumAmount,
                    MonthlyRevenue = monthlyRevenue,
                    YearlyRevenue = yearlyRevenue,
                    RevenueGrowthPercentage = revenueGrowthPercentage,
                    NewPoliciesThisMonth = newPoliciesThisMonth,
                    NewPoliciesThisWeek = newPoliciesThisWeek,
                    PoliciesByStatus = policiesByStatus,
                    PoliciesByType = policiesByType.ToDictionary(x => x.TypeName, x => x.Count),
                    PremiumByInsuranceCompany = premiumByInsuranceCompany.ToDictionary(x => x.CompanyName, x => x.TotalPremium)
                };
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting policy statistics for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<List<Policy>> GetRecentPoliciesAsync(int tenantId, int count = 10)
        {
            try
            {
                return await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                    .OrderByDescending(p => p.CreatedOn)
                    .Take(count)
                    .Include(p => p.Customer)
                    .Include(p => p.PolicyType)
                    .Include(p => p.InsuranceCompany)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting recent policies for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<Dictionary<PolicyStatus, int>> GetPoliciesByStatusAsync(int tenantId)
        {
            try
            {
                var statusCounts = await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId)
                    .GroupBy(p => p.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return statusCounts.ToDictionary(x => x.Status, x => x.Count);
            }
            catch (Exception ex)
            {
                //_logger?.LogError(ex, "Error getting policies by status for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int tenantId, int months = 12)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddMonths(-months);

                var monthlyRevenue = await _dbSet
                    .Where(p => !p.IsDeleted &&
                               p.TenantId == tenantId &&
                               p.Status == PolicyStatus.Active &&
                               p.StartDate >= startDate)
                    .GroupBy(p => new { p.StartDate.Year, p.StartDate.Month })
                    .Select(g => new {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Revenue = g.Sum(p => p.PremiumAmount)
                    })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToListAsync();

                return monthlyRevenue.ToDictionary(
                    x => $"{x.Year}-{x.Month:D2}",
                    x => x.Revenue);
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting revenue by month for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<List<Policy>> GetTopPoliciesByPremiumAsync(int tenantId, int count = 10)
        {
            try
            {
                return await _dbSet
                    .Where(p => !p.IsDeleted && p.TenantId == tenantId && p.Status == PolicyStatus.Active)
                    .OrderByDescending(p => p.PremiumAmount)
                    .Take(count)
                    .Include(p => p.Customer)
                    .Include(p => p.PolicyType)
                    .Include(p => p.InsuranceCompany)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting top policies by premium for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<List<Policy>> GetActivePoliciesByCustomerIdAsync(int customerId)
        {
            try
            {
                return await _dbSet
                    .Where(p => !p.IsDeleted &&
                               p.CustomerId == customerId &&
                               p.Status == PolicyStatus.Active)
                    .Include(p => p.PolicyType)
                    .Include(p => p.InsuranceCompany)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting active policies for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<DateTime?> GetLastActivityDateAsync(int customerId)
        {
            try
            {
                var lastPolicyDate = await _dbSet
                    .Where(p => !p.IsDeleted && p.CustomerId == customerId)
                    .OrderByDescending(p => p.ModifiedOn ?? p.CreatedOn)
                    .Select(p => p.ModifiedOn ?? p.CreatedOn)
                    .FirstOrDefaultAsync();

                return lastPolicyDate;
            }
            catch (Exception ex)
            {
               // _logger?.LogError(ex, "Error getting last activity date for customer {CustomerId}", customerId);
                return null;
            }
        }


    }
}