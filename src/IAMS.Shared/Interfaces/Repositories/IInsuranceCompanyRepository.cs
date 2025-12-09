using IAMS.Domain.Entities;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface IInsuranceCompanyRepository : IRepository<InsuranceCompany>
    {
        Task<IEnumerable<InsuranceCompany>> GetActiveCompaniesAsync();
        Task<InsuranceCompany?> GetByCodeAsync(string code);
        Task<InsuranceCompany?> GetByNameAsync(string name);
        Task<IEnumerable<InsuranceCompany>> GetCompaniesWithIntegrationAsync();
        Task<bool> CodeExistsAsync(string code, int? excludeId = null);
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
        Task<IEnumerable<CommissionRate>> GetCommissionRatesAsync(int companyId);
        Task<int> GetActiveCustomerCountAsync(int companyId);
        Task<decimal> GetTotalPremiumAmountAsync(int companyId);
        Task<int> GetActivePoliciesCountAsync(int id);
        Task<decimal> GetTotalCommissionsAsync(int id);
        Task<List<CurrencyBreakdown>> GetCurrencyBreakdownAsync(int companyId);
    }
}