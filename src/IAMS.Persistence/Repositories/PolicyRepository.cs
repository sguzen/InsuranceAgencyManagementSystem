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

        public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber && !p.IsDeleted);
        }

        public async Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<bool> PolicyNumberExistsAsync(string policyNumber, int tenantId, int? excludePolicyId = null)
        {
            var query = _dbSet.Where(p => !p.IsDeleted && p.PolicyNumber == policyNumber && p.TenantId == tenantId);

            if (excludePolicyId.HasValue)
            {
                query = query.Where(p => p.Id != excludePolicyId.Value);
            }

            return await query.AnyAsync();
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

            var totalCount = await query.CountAsync();

            var policies = await query
                .OrderByDescending(p => p.CreatedOn)
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

        public async Task<List<Policy>> GetActivePoliciesAsync(int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == PolicyStatus.Active)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByStatusAsync(PolicyStatus status, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == status)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetExpiringPoliciesAsync(int daysAhead, int tenantId)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.EndDate <= cutoffDate &&
                           p.EndDate >= DateTime.Now)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetExpiredPoliciesAsync(int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           (p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now))
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetRecentPoliciesAsync(int count, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetTopPoliciesByPremiumAsync(int count, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == PolicyStatus.Active)
                .OrderByDescending(p => p.PremiumAmount)
                .Take(count)
                .ToListAsync();
        }

        // Count methods
        public async Task<int> GetPolicyCountAsync(int tenantId)
        {
            return await _dbSet
                .CountAsync(p => p.TenantId == tenantId && !p.IsDeleted);
        }

        public async Task<int> GetActivePolicyCountAsync(int tenantId)
        {
            return await _dbSet
                .CountAsync(p => p.TenantId == tenantId && !p.IsDeleted && p.Status == PolicyStatus.Active);
        }

        public async Task<int> GetExpiringPolicyCountAsync(int daysAhead, int tenantId)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _dbSet
                .CountAsync(p => p.TenantId == tenantId &&
                               !p.IsDeleted &&
                               p.Status == PolicyStatus.Active &&
                               p.EndDate <= cutoffDate &&
                               p.EndDate >= DateTime.Now);
        }

        public async Task<int> GetExpiredPolicyCountAsync(int tenantId)
        {
            return await _dbSet
                .CountAsync(p => p.TenantId == tenantId &&
                               !p.IsDeleted &&
                               (p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now));
        }

        public async Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync(int tenantId)
        {
            return await _dbSet
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .GroupBy(p => p.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        // Revenue methods
        public async Task<decimal> GetMonthlyRevenueAsync(int tenantId, DateTime? month = null)
        {
            var targetMonth = month ?? DateTime.Now;
            var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _dbSet
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.StartDate >= startOfMonth &&
                           p.StartDate <= endOfMonth)
                .SumAsync(p => p.PremiumAmount);
        }

        public async Task<decimal> GetYearlyRevenueAsync(int tenantId, int? year = null)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var startOfYear = new DateTime(targetYear, 1, 1);
            var endOfYear = new DateTime(targetYear, 12, 31);

            return await _dbSet
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.StartDate >= startOfYear &&
                           p.StartDate <= endOfYear)
                .SumAsync(p => p.PremiumAmount);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int tenantId, int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);

            var revenues = await _dbSet
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.StartDate >= startDate)
                .GroupBy(p => new { p.StartDate.Year, p.StartDate.Month })
                .Select(g => new {
                    Key = $"{g.Key.Year}-{g.Key.Month:00}",
                    Revenue = g.Sum(p => p.PremiumAmount)
                })
                .ToDictionaryAsync(x => x.Key, x => x.Revenue);

            return revenues;
        }

        public async Task<decimal> GetTotalPremiumByCustomerAsync(int customerId, int tenantId)
        {
            return await _dbSet
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.CustomerId == customerId &&
                           p.Status == PolicyStatus.Active)
                .SumAsync(p => p.PremiumAmount);
        }

        // Business rule validation methods
        public async Task<bool> HasOverduePaymentsAsync(int policyId)
        {
            // This would need to check against a Payments table
            // Placeholder implementation
            return false;
        }

        public async Task<bool> CanBeCancelledAsync(int policyId)
        {
            var policy = await GetByIdAsync(policyId);
            return policy != null &&
                   policy.Status == PolicyStatus.Active &&
                   !policy.IsDeleted;
        }

        public async Task<bool> CanBeRenewedAsync(int policyId)
        {
            var policy = await GetByIdAsync(policyId);
            return policy != null &&
                   (policy.Status == PolicyStatus.Active || policy.Status == PolicyStatus.Expired) &&
                   !policy.IsDeleted;
        }

        // Advanced query methods
        public async Task<List<Policy>> GetPoliciesExpiringInDateRangeAsync(DateTime startDate, DateTime endDate, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.EndDate >= startDate &&
                           p.EndDate <= endDate)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> SearchPoliciesAsync(string searchTerm, int tenantId)
        {
            var search = searchTerm.ToLower();
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted &&
                           (p.PolicyNumber.ToLower().Contains(search) ||
                            p.Customer.FirstName.ToLower().Contains(search) ||
                            p.Customer.LastName.ToLower().Contains(search) ||
                            p.InsuranceCompany.Name.ToLower().Contains(search) ||
                            p.PolicyType.Name.ToLower().Contains(search)))
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.InsuranceCompanyId == insuranceCompanyId)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByPolicyTypeAsync(int policyTypeId, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.PolicyTypeId == policyTypeId)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        // Reporting methods
        public async Task<List<Policy>> GetPoliciesForReportAsync(DateTime? startDate, DateTime? endDate, PolicyStatus? status, int tenantId)
        {
            var query = _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId && !p.IsDeleted);

            if (startDate.HasValue)
                query = query.Where(p => p.StartDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.StartDate <= endDate.Value);

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return await query
                .OrderBy(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetPolicyAnalyticsAsync(int tenantId)
        {
            var totalPolicies = await GetPolicyCountAsync(tenantId);
            var activePolicies = await GetActivePolicyCountAsync(tenantId);
            var monthlyRevenue = await GetMonthlyRevenueAsync(tenantId);
            var yearlyRevenue = await GetYearlyRevenueAsync(tenantId);

            return new Dictionary<string, object>
            {
                { "TotalPolicies", totalPolicies },
                { "ActivePolicies", activePolicies },
                { "MonthlyRevenue", monthlyRevenue },
                { "YearlyRevenue", yearlyRevenue },
                { "AveragePremium", totalPolicies > 0 ? yearlyRevenue / totalPolicies : 0 }
            };
        }

        // Integration support methods
        public async Task<List<Policy>> GetPoliciesModifiedAfterAsync(DateTime modifiedDate, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => p.TenantId == tenantId &&
                           !p.IsDeleted &&
                           p.ModifiedOn > modifiedDate)
                .OrderBy(p => p.ModifiedOn)
                .ToListAsync();
        }

        public async Task<Policy?> GetPolicyByExternalIdAsync(string externalId, int insuranceCompanyId, int tenantId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .FirstOrDefaultAsync(p => p.TenantId == tenantId &&
                                        !p.IsDeleted &&
                                        //p.ExternalReference == externalId &&
                                        p.InsuranceCompanyId == insuranceCompanyId);
        }

        public async Task UpdateExternalSyncStatusAsync(int policyId, bool synced, DateTime? lastSyncDate = null)
        {
            var policy = await GetByIdAsync(policyId);
            if (policy != null)
            {
                // Add sync status fields to Policy entity if needed
                // policy.IsSynced = synced;
                // policy.LastSyncDate = lastSyncDate ?? DateTime.UtcNow;
                Update(policy);
            }
        }

        public async Task<PagedResult<Policy>> GetPoliciesByStatusPagedAsync(PolicyStatus status, int pageNumber, int pageSize)
        {
            var query = _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Where(p => !p.IsDeleted && p.Status == status);

            var totalCount = await query.CountAsync();

            var policies = await query
                .OrderByDescending(p => p.CreatedOn)
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

        public Task<int> GetExpiringPoliciesCountAsync(int tenantId, int daysAhead)
        {
            throw new NotImplementedException();
        }
    }
}