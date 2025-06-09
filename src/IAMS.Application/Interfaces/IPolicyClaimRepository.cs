using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces
{
    public interface IPolicyClaimRepository : IRepository<PolicyClaim>
    {
        Task<IEnumerable<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId);
        Task<IEnumerable<PolicyClaim>> GetClaimsByStatusAsync(ClaimStatus status);
        Task<IEnumerable<PolicyClaim>> GetClaimsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalClaimAmountByPolicyIdAsync(int policyId);
    }
}
