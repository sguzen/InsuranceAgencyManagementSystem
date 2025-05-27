using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        Task<IEnumerable<Policy>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<IEnumerable<Policy>> GetExpiringPoliciesAsync(DateTime date);
        Task<IEnumerable<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId);
    }
}