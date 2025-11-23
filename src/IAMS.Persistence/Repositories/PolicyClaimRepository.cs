using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IAMS.Persistence.Repositories
{
    public class PolicyClaimRepository : Repository<PolicyClaim>, IPolicyClaimRepository
    {
        public PolicyClaimRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pc => pc.PolicyId == policyId && !pc.IsDeleted)
                .OrderByDescending(pc => pc.ClaimDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyClaim>> GetClaimsByStatusAsync(ClaimStatus status)
        {
            return await _dbSet
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pc => pc.Status == status && !pc.IsDeleted)
                .OrderByDescending(pc => pc.ClaimDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyClaim>> GetClaimsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pc => !pc.IsDeleted &&
                            pc.ClaimDate >= fromDate &&
                            pc.ClaimDate <= toDate)
                .OrderByDescending(pc => pc.ClaimDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalClaimAmountByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .Where(pc => pc.PolicyId == policyId &&
                            !pc.IsDeleted &&
                            pc.Status == ClaimStatus.Approved)
                .SumAsync(pc => pc.ClaimAmount);
        }

        public async Task<DateTime?> GetLastClaimDateAsync(int customerId)
        {
            return await _dbSet
                .Include(pc => pc.Policy)
                .Where(pc => !pc.IsDeleted &&
                            pc.Policy.CustomerId == customerId)
                .OrderByDescending(pc => pc.ClaimDate)
                .Select(pc => (DateTime?)pc.ClaimDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByPolicyIdAsync(int policyId)
        {
            return await _dbSet
                .AnyAsync(pc => pc.PolicyId == policyId && !pc.IsDeleted);
        }

        public async Task<List<PolicyClaim>> GetClaimsByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.Customer)
                .Include(pc => pc.Policy)
                    .ThenInclude(p => p.InsuranceCompany)
                .Where(pc => !pc.IsDeleted && pc.Policy.CustomerId == customerId)
                .OrderByDescending(pc => pc.ClaimDate)
                .ToListAsync();
        }

        public async Task<int> GetClaimCountByStatusAsync(ClaimStatus status)
        {
            return await _dbSet
                .Where(pc => pc.Status == status && !pc.IsDeleted)
                .CountAsync();
        }

        public async Task<decimal> GetTotalClaimAmountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet.Where(pc => !pc.IsDeleted);

            if (startDate.HasValue)
            {
                query = query.Where(pc => pc.ClaimDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(pc => pc.ClaimDate <= endDate.Value);
            }

            return await query.SumAsync(pc => pc.ClaimAmount);
        }
    }
}