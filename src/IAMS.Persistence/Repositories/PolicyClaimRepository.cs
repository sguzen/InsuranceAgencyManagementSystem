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

        public Task<DateTime?> GetLastClaimDateAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsByPolicyIdAsync(int policyId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Claim>> GetClaimsByCustomerIdAsync(int customerId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetClaimCountByStatusAsync(ClaimStatus status, int tenantId)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> GetTotalClaimAmountAsync(int tenantId, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException();
        }
    }
}