using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces
{
    public interface ICustomerInsuranceCompanyRepository : IRepository<CustomerInsuranceCompany>
    {
        Task<IEnumerable<CustomerInsuranceCompany>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<CustomerInsuranceCompany>> GetByInsuranceCompanyIdAsync(int companyId);
        Task<CustomerInsuranceCompany?> GetMappingAsync(int customerId, int companyId);
        Task<string?> GetExternalCustomerIdAsync(int customerId, int companyId);
    }
}