using IAMS.Application.Models;
using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IInsuranceCompanyRepository : IRepository<InsuranceCompany>
    {
        Task<IEnumerable<InsuranceCompany>> GetActiveCompaniesAsync();
        Task<PagedResult<InsuranceCompany>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<InsuranceCompany?> GetByNameAsync(string name);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<int> GetActivePoliciesCountAsync(object id);
        Task<decimal> GetTotalPremiumsAsync(object id);
        Task<decimal> GetTotalCommissionsAsync(object id);
        Task<InsuranceCompany> GetByIdWithDetailsAsync(int id);
    }
}