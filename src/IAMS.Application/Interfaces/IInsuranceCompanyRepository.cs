using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces
{
    public interface IInsuranceCompanyRepository : IRepository<InsuranceCompany>
    {
        Task<IEnumerable<InsuranceCompany>> GetActiveCompaniesAsync();
        Task<InsuranceCompany?> GetByNameAsync(string name);
        Task<IEnumerable<InsuranceCompany>> GetCompaniesByCustomerIdAsync(int customerId);
        Task<bool> HasPoliciesAsync(int companyId);
    }
}