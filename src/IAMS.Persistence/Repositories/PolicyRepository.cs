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

        public Task<DateTime?> GetLastActivityDateAsync(int customerId)
        {
            throw new NotImplementedException();
        }
    }
}