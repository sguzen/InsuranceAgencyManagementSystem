using IAMS.Shared.DTOs.Agency;
using IAMS.Shared.Models;
using System.Net.Http.Json;

namespace IAMS.Admin.Services.ApiClient
{
    public interface IInsuranceCompaniesApiClient
    {
        Task<Result<List<AgencyInsuranceCompanyDto>>> GetAgencyInsuranceCompaniesAsync(int agencyId);
        Task<Result<AgencyInsuranceCompanyDto>> GetAgencyInsuranceCompanyAsync(int agencyId, int id);
        Task<Result<AgencyInsuranceCompanyDto>> AddInsuranceCompanyAsync(int agencyId, CreateAgencyInsuranceCompanyDto dto);
        Task<Result> UpdateInsuranceCompanyAsync(int agencyId, int id, UpdateAgencyInsuranceCompanyDto dto);
        Task<Result> RemoveInsuranceCompanyAsync(int agencyId, int id);
        Task<Result<List<InsuranceCompanySummaryDto>>> GetAllInsuranceCompaniesAsync();
        Task<Result<TestConnectionResult>> TestConnectionAsync(int agencyId, int id);
    }

    public class TestConnectionResult
    {
        public string Message { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class InsuranceCompaniesApiClient : BaseApiClient, IInsuranceCompaniesApiClient
    {
        public InsuranceCompaniesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<AgencyInsuranceCompanyDto>>> GetAgencyInsuranceCompaniesAsync(int agencyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/agencies/{agencyId}/insurance-companies");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<AgencyInsuranceCompanyDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<List<AgencyInsuranceCompanyDto>>(_jsonOptions);
                return result != null
                    ? Result<List<AgencyInsuranceCompanyDto>>.Success(result)
                    : Result<List<AgencyInsuranceCompanyDto>>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<List<AgencyInsuranceCompanyDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<AgencyInsuranceCompanyDto>> GetAgencyInsuranceCompanyAsync(int agencyId, int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/agencies/{agencyId}/insurance-companies/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<AgencyInsuranceCompanyDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<AgencyInsuranceCompanyDto>(_jsonOptions);
                return result != null
                    ? Result<AgencyInsuranceCompanyDto>.Success(result)
                    : Result<AgencyInsuranceCompanyDto>.Failure("Not found");
            }
            catch (Exception ex)
            {
                return Result<AgencyInsuranceCompanyDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<AgencyInsuranceCompanyDto>> AddInsuranceCompanyAsync(int agencyId, CreateAgencyInsuranceCompanyDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/agencies/{agencyId}/insurance-companies", dto, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<AgencyInsuranceCompanyDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<AgencyInsuranceCompanyDto>(_jsonOptions);
                return result != null
                    ? Result<AgencyInsuranceCompanyDto>.Success(result, "Sigorta şirketi başarıyla eklendi")
                    : Result<AgencyInsuranceCompanyDto>.Success("Sigorta şirketi başarıyla eklendi");
            }
            catch (Exception ex)
            {
                return Result<AgencyInsuranceCompanyDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> UpdateInsuranceCompanyAsync(int agencyId, int id, UpdateAgencyInsuranceCompanyDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/agencies/{agencyId}/insurance-companies/{id}", dto, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Bağlantı bilgileri başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> RemoveInsuranceCompanyAsync(int agencyId, int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/agencies/{agencyId}/insurance-companies/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Sigorta şirketi başarıyla kaldırıldı");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<List<InsuranceCompanySummaryDto>>> GetAllInsuranceCompaniesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/insurance-companies/all");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<InsuranceCompanySummaryDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<List<InsuranceCompanySummaryDto>>(_jsonOptions);
                return result != null
                    ? Result<List<InsuranceCompanySummaryDto>>.Success(result)
                    : Result<List<InsuranceCompanySummaryDto>>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<List<InsuranceCompanySummaryDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<TestConnectionResult>> TestConnectionAsync(int agencyId, int id)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/agencies/{agencyId}/insurance-companies/{id}/test-connection", new { }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<TestConnectionResult>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<TestConnectionResult>(_jsonOptions);
                return result != null
                    ? Result<TestConnectionResult>.Success(result)
                    : Result<TestConnectionResult>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<TestConnectionResult>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }
    }
}
