using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface IPolicyClaimRepository : IRepository<PolicyClaim>
    {
        Task<bool> ExistsByPolicyIdAsync(int policyId);
        Task<IEnumerable<PolicyClaim>> GetClaimsByPolicyIdAsync(int policyId);
        Task<IEnumerable<PolicyClaim>> GetClaimsByStatusAsync(ClaimStatus status);
        Task<IEnumerable<PolicyClaim>> GetClaimsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<decimal> GetTotalClaimAmountByPolicyIdAsync(int policyId);
        Task<DateTime?> GetLastClaimDateAsync(int customerId);
        Task<List<PolicyClaim>> GetClaimsByCustomerIdAsync(int customerId);
        Task<int> GetClaimCountByStatusAsync(ClaimStatus status);
        Task<decimal> GetTotalClaimAmountAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
