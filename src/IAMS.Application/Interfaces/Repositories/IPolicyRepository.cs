using IAMS.Application.DTOs.Policy;
using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<Policy?> GetByIdWithDetailsAsync(int id);
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber, int tenantId);
        Task<List<Policy>> GetActivePoliciesAsync(int tenantId);
        Task<List<Policy>> GetExpiringPoliciesAsync(int tenantId, int daysAhead = 30);
        Task<List<Policy>> GetExpiredPoliciesAsync(int tenantId);
        Task<List<Policy>> GetPoliciesByCustomerAsync(int customerId);
        Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId);
        Task<(List<Policy> policies, int totalCount)> GetPagedAsync(PolicyQueryParams queryParams);
        Task<bool> PolicyNumberExistsAsync(string policyNumber, int tenantId, int? excludePolicyId = null);
        Task<decimal> GetTotalPremiumsByCustomerAsync(int customerId);
        Task<decimal> GetTotalCommissionsByCustomerAsync(int customerId);
    }
}