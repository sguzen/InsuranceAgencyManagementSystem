using IAMS.Domain.Entities;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface ICommissionRateRepository : IRepository<CommissionRate>
    {
        Task<CommissionRate?> GetByPolicyTypeAndCompanyAsync(int policyTypeId, int companyId);
        Task<IEnumerable<CommissionRate>> GetByInsuranceCompanyAsync(int companyId);
        Task<IEnumerable<CommissionRate>> GetActiveFatesAsync();
    }
}