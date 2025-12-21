using IAMS.Application.DTOs.InsuranceCompany;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;

namespace IAMS.Web.Services.ApiClient
{
    public interface IInsuranceCompaniesApiClient
    {
        Task<Result<PagedResult<InsuranceCompanyDto>>> GetInsuranceCompaniesAsync(InsuranceCompanyQueryParams queryParams);
        Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByIdAsync(int id);
        Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByNameAsync(string name);
        Task<Result<List<InsuranceCompanyDto>>> GetActiveInsuranceCompaniesAsync();
        Task<Result<List<InsuranceCompanyDto>>> GetActiveAsync();
        Task<Result<InsuranceCompanyDto>> CreateInsuranceCompanyAsync(CreateInsuranceCompanyDto companyDto);
        Task<Result<InsuranceCompanyDto>> UpdateInsuranceCompanyAsync(int id, UpdateInsuranceCompanyDto companyDto);
        Task<Result> DeleteInsuranceCompanyAsync(int id);
    }

    public class InsuranceCompaniesApiClient : BaseApiClient, IInsuranceCompaniesApiClient
    {
        public InsuranceCompaniesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<PagedResult<InsuranceCompanyDto>>> GetInsuranceCompaniesAsync(InsuranceCompanyQueryParams queryParams)
        {
            var queryString = $"?pageNumber={queryParams.PageNumber}&pageSize={queryParams.PageSize}";
            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(queryParams.SearchTerm)}";
            if (queryParams.IsActive.HasValue)
                queryString += $"&isActive={queryParams.IsActive.Value}";

            return await GetAsync<PagedResult<InsuranceCompanyDto>>($"api/insurancecompanies{queryString}");
        }

        public async Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByIdAsync(int id)
        {
            return await GetAsync<InsuranceCompanyDto>($"api/insurancecompanies/{id}");
        }

        public async Task<Result<InsuranceCompanyDto>> GetInsuranceCompanyByNameAsync(string name)
        {
            return await GetAsync<InsuranceCompanyDto>($"api/insurancecompanies/by-name/{Uri.EscapeDataString(name)}");
        }

        public async Task<Result<List<InsuranceCompanyDto>>> GetActiveInsuranceCompaniesAsync()
        {
            return await GetAsync<List<InsuranceCompanyDto>>("api/insurancecompanies/active");
        }

        public async Task<Result<List<InsuranceCompanyDto>>> GetActiveAsync()
        {
            return await GetAsync<List<InsuranceCompanyDto>>("api/insurancecompanies/active");
        }

        public async Task<Result<InsuranceCompanyDto>> CreateInsuranceCompanyAsync(CreateInsuranceCompanyDto companyDto)
        {
            return await PostAsync<InsuranceCompanyDto>("api/insurancecompanies", companyDto);
        }

        public async Task<Result<InsuranceCompanyDto>> UpdateInsuranceCompanyAsync(int id, UpdateInsuranceCompanyDto companyDto)
        {
            return await PutAsync<InsuranceCompanyDto>($"api/insurancecompanies/{id}", companyDto);
        }

        public async Task<Result> DeleteInsuranceCompanyAsync(int id)
        {
            return await DeleteAsync($"api/insurancecompanies/{id}");
        }
    }
}
