using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<IEnumerable<Policy>> GetPoliciesByCompanyIdAsync(int companyId);
        Task<IEnumerable<Policy>> GetPoliciesByStatusAsync(PolicyStatus status);
        Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(DateTime date);
        Task<PagedResult<Policy>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber);
        Task<decimal> GetTotalPremiumByCustomerAsync(int customerId);
        Task<decimal> GetTotalCommissionByCustomerAsync(int customerId);
        Task<DateTime?> GetLastActivityDateAsync(int customerId);
    }
}