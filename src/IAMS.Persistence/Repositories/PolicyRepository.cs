using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;
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

        public async Task<Policy?> GetPolicyByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.Brand)
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.Model)
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(p => p.Vehicle)
                    .ThenInclude(v => v.Currency)
                .Include(p => p.Currency)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber)
        {
            // Gets the main policy (InnerCode = "000") for a given policy number
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber && p.InnerCode == "000" && !p.IsDeleted);
        }

        public async Task<Policy?> GetByPolicyNumberAndInnerCodeAsync(string policyNumber, string innerCode)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber && p.InnerCode == innerCode && !p.IsDeleted);
        }

        public async Task<List<Policy>> GetAllByPolicyNumberAsync(string policyNumber)
        {
            // Gets all policies (main + endorsements) for a given policy number
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => p.PolicyNumber == policyNumber && !p.IsDeleted)
                .OrderBy(p => p.InnerCode)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<bool> PolicyNumberExistsAsync(string policyNumber, int? excludePolicyId = null)
        {
            // Checks if a main policy (InnerCode = "000") exists with this policy number
            var query = _dbSet.Where(p => !p.IsDeleted && p.PolicyNumber == policyNumber && p.InnerCode == "000");

            if (excludePolicyId.HasValue)
            {
                query = query.Where(p => p.Id != excludePolicyId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> PolicyNumberAndInnerCodeExistsAsync(string policyNumber, string innerCode, int? excludePolicyId = null)
        {
            var query = _dbSet.Where(p => !p.IsDeleted && p.PolicyNumber == policyNumber && p.InnerCode == innerCode);

            if (excludePolicyId.HasValue)
            {
                query = query.Where(p => p.Id != excludePolicyId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<PagedResult<Policy>> GetPoliciesAsync(PolicyQueryParams queryParams)
        {
            var query = _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Include(p => p.Vehicle)
                .Where(p => !p.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var search = queryParams.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.PolicyNumber.ToLower().Contains(search) ||
                    p.Customer.FirstName.ToLower().Contains(search) ||
                    p.Customer.LastName.ToLower().Contains(search) ||
                    p.InsuranceCompany.Name.ToLower().Contains(search) ||
                    p.PolicyType.Name.ToLower().Contains(search));
            }

            // Apply status filter
            if (queryParams.Status.HasValue)
            {
                query = query.Where(p => p.Status == queryParams.Status.Value);
            }

            // Apply customer filter
            if (queryParams.CustomerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == queryParams.CustomerId.Value);
            }

            // Apply insurance company filter
            if (queryParams.InsuranceCompanyId.HasValue)
            {
                query = query.Where(p => p.InsuranceCompanyId == queryParams.InsuranceCompanyId.Value);
            }

            // Apply policy type filter
            if (queryParams.PolicyTypeId.HasValue)
            {
                query = query.Where(p => p.PolicyTypeId == queryParams.PolicyTypeId.Value);
            }

            // Apply start date range filter
            if (queryParams.StartDateFrom.HasValue)
            {
                query = query.Where(p => p.StartDate >= queryParams.StartDateFrom.Value);
            }

            if (queryParams.StartDateTo.HasValue)
            {
                query = query.Where(p => p.StartDate <= queryParams.StartDateTo.Value);
            }

            // Apply end date range filter
            if (queryParams.EndDateFrom.HasValue)
            {
                query = query.Where(p => p.EndDate >= queryParams.EndDateFrom.Value);
            }

            if (queryParams.EndDateTo.HasValue)
            {
                query = query.Where(p => p.EndDate <= queryParams.EndDateTo.Value);
            }

            // Apply expiring filter (next 30 days)
            if (queryParams.IsExpiring.HasValue && queryParams.IsExpiring.Value)
            {
                var cutoffDate = DateTime.Now.AddDays(30);
                query = query.Where(p => p.Status == PolicyStatus.Active &&
                                        p.EndDate <= cutoffDate &&
                                        p.EndDate >= DateTime.Now);
            }

            // Apply expired filter
            if (queryParams.IsExpired.HasValue && queryParams.IsExpired.Value)
            {
                query = query.Where(p => p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now);
            }

            // Apply premium range filters
            if (queryParams.MinPremium.HasValue)
            {
                query = query.Where(p => p.PremiumAmount >= queryParams.MinPremium.Value);
            }

            if (queryParams.MaxPremium.HasValue)
            {
                query = query.Where(p => p.PremiumAmount <= queryParams.MaxPremium.Value);
            }

            var totalCount = await query.CountAsync();
            var policies = new List<Policy>();
            if (totalCount > 0)
            {
                policies = await query
                    .OrderByDescending(p => p.CreatedOn)
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();
            }

            return new PagedResult<Policy>
            {
                Items = policies,
                TotalCount = totalCount,
                PageNumber = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<PagedResult<Policy>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            var query = _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
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
            var policies = new List<Policy>();
            if (totalCount > 0)
            {
                policies = await query
                .OrderByDescending(p => p.CreatedOn)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            }


            return new PagedResult<Policy>
                {
                    Items = policies,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
        }

        public async Task<List<Policy>> GetActivePoliciesAsync()
        {
            return await _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByStatusAsync(PolicyStatus status)
        {
            return await _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted && p.Status == status)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetExpiringPoliciesAsync(int daysAhead)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.EndDate <= cutoffDate &&
                           p.EndDate >= DateTime.Now)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetExpiredPoliciesAsync()
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted &&
                           (p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now))
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetRecentPoliciesAsync(int count)
        {
            return await _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetTopPoliciesByPremiumAsync(int count)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted && p.Status == PolicyStatus.Active)
                .OrderByDescending(p => p.PremiumAmount)
                .Take(count)
                .ToListAsync();
        }

        // Count methods
        public async Task<int> GetPolicyCountAsync()
        {
            return await _dbSet
                .CountAsync(p =>!p.IsDeleted);
        }

        public async Task<int> GetActivePolicyCountAsync()
        {
            return await _dbSet
                .CountAsync(p => !p.IsDeleted && p.Status == PolicyStatus.Active);
        }

        public async Task<int> GetExpiringPolicyCountAsync(int daysAhead)
        {
            var cutoffDate = DateTime.Now.AddDays(daysAhead);
            return await _dbSet
                .CountAsync(p =>
                               !p.IsDeleted &&
                               p.Status == PolicyStatus.Active &&
                               p.EndDate <= cutoffDate &&
                               p.EndDate >= DateTime.Now);
        }

        public async Task<int> GetExpiredPolicyCountAsync()
        {
            return await _dbSet
                .CountAsync(p => 
                               !p.IsDeleted &&
                               (p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now));
        }

        public async Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync()
        {
            return await _dbSet
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        // Revenue methods
        public async Task<decimal> GetMonthlyRevenueAsync(DateTime? month = null)
        {
            var targetMonth = month ?? DateTime.Now;
            var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Calculate revenue from policies that are active during the month
            // Policy must: start on or before month end AND end on or after month start
            return await _dbSet
                .Where(p =>
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                          // p.InnerCode == "000" && // Only count main policies, not endorsements
                           p.StartDate <= endOfMonth &&
                           p.EndDate >= startOfMonth)
                .SumAsync(p => p.PremiumAmount);
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyRevenueByCurrencyAsync(DateTime? month = null)
        {
            var targetMonth = month ?? DateTime.Now;
            var startOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Calculate revenue from policies that are active during the month
            // Policy must: start on or before month end AND end on or after month start
            return await _dbSet
                .Include(p => p.Currency)
                .Where(p =>
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.InnerCode == "000" && // Only count main policies, not endorsements
                           p.StartDate <= endOfMonth &&
                           p.EndDate >= startOfMonth)
                .GroupBy(p => p.Currency.Code)
                .Select(g => new { Currency = g.Key, Total = g.Sum(p => p.PremiumAmount) })
                .ToDictionaryAsync(x => x.Currency, x => x.Total);
        }

        public async Task<decimal> GetYearlyRevenueAsync(int? year = null)
        {
            var targetYear = year ?? DateTime.Now.Year;
            var startOfYear = new DateTime(targetYear, 1, 1);
            var endOfYear = new DateTime(targetYear, 12, 31);

            // Calculate revenue from policies that are active during the year
            // Policy must: start on or before year end AND end on or after year start
            return await _dbSet
                .Where(p =>
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.InnerCode == "000" && // Only count main policies, not endorsements
                           p.StartDate <= endOfYear &&
                           p.EndDate >= startOfYear)
                .SumAsync(p => p.PremiumAmount);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync( int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);

            var revenues = await _dbSet
                .Where(p => 
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

        public async Task<decimal> GetTotalPremiumByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(p => 
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
        public async Task<List<Policy>> GetPoliciesExpiringInDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p =>
                           !p.IsDeleted &&
                           p.Status == PolicyStatus.Active &&
                           p.EndDate >= startDate &&
                           p.EndDate <= endDate)
                .OrderBy(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<List<Policy>> SearchPoliciesAsync(string searchTerm)
        {
            var search = searchTerm.ToLower();
            return await _dbSet
                .AsNoTracking() // OPTIMIZED: Read-only query
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted &&
                           (p.PolicyNumber.ToLower().Contains(search) ||
                            p.Customer.FirstName.ToLower().Contains(search) ||
                            p.Customer.LastName.ToLower().Contains(search) ||
                            p.InsuranceCompany.Name.ToLower().Contains(search) ||
                            p.PolicyType.Name.ToLower().Contains(search)))
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p =>
                           !p.IsDeleted &&
                           p.InsuranceCompanyId == insuranceCompanyId)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetPoliciesByPolicyTypeAsync(int policyTypeId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.Currency)
                .Where(p =>
                           !p.IsDeleted &&
                           p.PolicyTypeId == policyTypeId)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        // Reporting methods
        public async Task<List<Policy>> GetPoliciesForReportAsync(DateTime? startDate, DateTime? endDate, PolicyStatus? status)
        {
            var query = _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => !p.IsDeleted);

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

        public async Task<Dictionary<string, object>> GetPolicyAnalyticsAsync()
        {
            var totalPolicies = await GetPolicyCountAsync();
            var activePolicies = await GetActivePolicyCountAsync();
            var monthlyRevenue = await GetMonthlyRevenueAsync();
            var yearlyRevenue = await GetYearlyRevenueAsync();

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
        public async Task<List<Policy>> GetPoliciesModifiedAfterAsync(DateTime modifiedDate)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p =>
                           !p.IsDeleted &&
                           p.ModifiedOn > modifiedDate)
                .OrderBy(p => p.ModifiedOn)
                .ToListAsync();
        }

        public async Task<Policy?> GetPolicyByExternalIdAsync(string externalId, int insuranceCompanyId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .FirstOrDefaultAsync(p =>
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
                .Include(p => p.Currency)
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

        public async Task<int> GetExpiringPoliciesCountAsync(int daysAhead)
        {
            var today = DateTime.Today;
            var futureDate = today.AddDays(daysAhead);

            return await _dbSet
                .Where(p => !p.IsDeleted)
                .CountAsync(p => p.Status == PolicyStatus.Active &&
                                p.EndDate.Date <= futureDate &&
                                p.EndDate.Date >= today);
        }

        public async Task<List<Policy>> GetByVehicleIdAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Vehicle)
                .Include(p => p.Currency)
                .Where(p => p.VehicleId == id && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetEndorsementsByOriginalPolicyIdAsync(int originalPolicyId)
        {
            return await _dbSet
                .Include(p => p.Customer)
                .Include(p => p.InsuranceCompany)
                .Include(p => p.PolicyType)
                .Include(p => p.Currency)
                .Where(p => p.OriginalPolicyId == originalPolicyId && p.InnerCode != "000" && !p.IsDeleted)
                .OrderBy(p => p.InnerCode)
                .ToListAsync();
        }

        public async Task<int> GetEndorsementCountAsync(int originalPolicyId)
        {
            return await _dbSet
                .Where(p => p.OriginalPolicyId == originalPolicyId && p.InnerCode != "000" && !p.IsDeleted)
                .CountAsync();
        }

        // ========================================
        // IQueryable Methods for AutoMapper ProjectTo
        // Return IQueryable to allow Application layer to project to DTOs
        // Keeps clean architecture - no Application -> Persistence dependency
        // ========================================

        public IQueryable<Policy> GetPoliciesQuery(PolicyQueryParams queryParams)
        {
            var query = _dbSet.Where(p => !p.IsDeleted);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var search = queryParams.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.PolicyNumber.ToLower().Contains(search) ||
                    p.Customer.FirstName.ToLower().Contains(search) ||
                    p.Customer.LastName.ToLower().Contains(search) ||
                    p.InsuranceCompany.Name.ToLower().Contains(search) ||
                    p.PolicyType.Name.ToLower().Contains(search));
            }

            // Apply filters
            if (queryParams.Status.HasValue)
                query = query.Where(p => p.Status == queryParams.Status.Value);

            if (queryParams.CustomerId.HasValue)
                query = query.Where(p => p.CustomerId == queryParams.CustomerId.Value);

            if (queryParams.InsuranceCompanyId.HasValue)
                query = query.Where(p => p.InsuranceCompanyId == queryParams.InsuranceCompanyId.Value);

            if (queryParams.PolicyTypeId.HasValue)
                query = query.Where(p => p.PolicyTypeId == queryParams.PolicyTypeId.Value);

            if (queryParams.StartDateFrom.HasValue)
                query = query.Where(p => p.StartDate >= queryParams.StartDateFrom.Value);

            if (queryParams.StartDateTo.HasValue)
                query = query.Where(p => p.StartDate <= queryParams.StartDateTo.Value);

            if (queryParams.EndDateFrom.HasValue)
                query = query.Where(p => p.EndDate >= queryParams.EndDateFrom.Value);

            if (queryParams.EndDateTo.HasValue)
                query = query.Where(p => p.EndDate <= queryParams.EndDateTo.Value);

            if (queryParams.IsExpiring.HasValue && queryParams.IsExpiring.Value)
            {
                var cutoffDate = DateTime.Now.AddDays(30);
                query = query.Where(p => p.Status == PolicyStatus.Active &&
                                        p.EndDate <= cutoffDate &&
                                        p.EndDate >= DateTime.Now);
            }

            if (queryParams.IsExpired.HasValue && queryParams.IsExpired.Value)
                query = query.Where(p => p.Status == PolicyStatus.Expired || p.EndDate < DateTime.Now);

            if (queryParams.MinPremium.HasValue)
                query = query.Where(p => p.PremiumAmount >= queryParams.MinPremium.Value);

            if (queryParams.MaxPremium.HasValue)
                query = query.Where(p => p.PremiumAmount <= queryParams.MaxPremium.Value);

            // Apply pagination
            return query
                .OrderByDescending(p => p.CreatedOn)
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize);
        }

        public IQueryable<Policy> SearchPoliciesQuery(string searchTerm)
        {
            var search = searchTerm.ToLower();
            return _dbSet
                .Where(p => !p.IsDeleted &&
                           (p.PolicyNumber.ToLower().Contains(search) ||
                            p.Customer.FirstName.ToLower().Contains(search) ||
                            p.Customer.LastName.ToLower().Contains(search) ||
                            p.InsuranceCompany.Name.ToLower().Contains(search) ||
                            p.PolicyType.Name.ToLower().Contains(search)))
                .OrderByDescending(p => p.CreatedOn);
        }

        public IQueryable<Policy> GetRecentPoliciesQuery(int count)
        {
            return _dbSet
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedOn)
                .Take(count);
        }

        public IQueryable<Policy> GetTopPoliciesByPremiumQuery(int count)
        {
            return _dbSet
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.PremiumAmount)
                .Take(count);
        }
    }
}