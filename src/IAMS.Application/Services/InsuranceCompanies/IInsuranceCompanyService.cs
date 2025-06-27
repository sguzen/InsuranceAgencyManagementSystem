using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;

namespace IAMS.Application.Services.InsuranceCompanies
{
    public interface IInsuranceCompanyService
    {
        Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByIdAsync(int id);
        Task<Result<PagedResult<InsuranceCompanyDto>>> GetInsuranceCompaniesAsync(InsuranceCompanyQueryParams queryParams);
        Task<Result<InsuranceCompanyDto>> CreateInsuranceCompanyAsync(CreateInsuranceCompanyDto createCompanyDto);
        Task<Result<InsuranceCompanyDto>> UpdateInsuranceCompanyAsync(int id, UpdateInsuranceCompanyDto updateCompanyDto);
        Task<Result> DeleteInsuranceCompanyAsync(int id);
        Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByNameAsync(string name);
        Task<Result<List<InsuranceCompanyDto>>> GetActiveInsuranceCompaniesAsync();
    }
}