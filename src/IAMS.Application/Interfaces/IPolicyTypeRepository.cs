using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces
{
    public interface IPolicyTypeRepository : IRepository<PolicyType>
    {
        Task<IEnumerable<PolicyType>> GetActiveTypesAsync();
        Task<PolicyType?> GetByNameAsync(string name);
        Task<IEnumerable<PolicyType>> GetTypesByInsuranceCompanyAsync(int companyId);
    }
}