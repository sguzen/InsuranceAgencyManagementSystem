using IAMS.Domain.Entities;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface IPolicyTypeRepository : IRepository<PolicyType>
    {
        Task<IEnumerable<PolicyType>> GetActiveTypesAsync();
        Task<PolicyType?> GetByNameAsync(string name);
        Task<PolicyType?> GetByCodeAsync(string code);
        Task<IEnumerable<PolicyType>> GetTypesByInsuranceCompanyAsync(int companyId);
    }
}