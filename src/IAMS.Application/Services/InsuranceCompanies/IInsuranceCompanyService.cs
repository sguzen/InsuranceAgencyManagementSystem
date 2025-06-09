using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;

namespace IAMS.Application.Services.InsuranceCompanies
{
    public interface IInsuranceCompanyService
    {
        Task<List<InsuranceCompanyDto>> GetAllAsync();
        Task<InsuranceCompanyDto?> GetByIdAsync(int id);
        Task<InsuranceCompanyDto> CreateAsync(CreateInsuranceCompanyDto companyDto);
        Task UpdateAsync(int id, UpdateInsuranceCompanyDto companyDto);
        Task DeleteAsync(int id);
        Task<List<InsuranceCompanyDto>> GetActiveCompaniesAsync();
        Task<PagedResult<InsuranceCompanyDto>> GetCompaniesPagedAsync(int pageNumber, int pageSize, string searchTerm = null);
    }
}