using IAMS.Shared.DTOs.MasterData;
using IAMS.Shared.Models;
using System.Net.Http.Json;

namespace IAMS.Admin.Services.ApiClient
{
    public interface IMasterInsuranceCompaniesApiClient
    {
        Task<Result<List<MasterInsuranceCompanyDto>>> GetAllAsync(string? search = null, bool? isActive = null);
        Task<Result<List<InsuranceCompanySelectDto>>> GetForSelectAsync();
        Task<Result<MasterInsuranceCompanyDto>> GetByIdAsync(int id);
        Task<Result<MasterInsuranceCompanyDto>> CreateAsync(CreateMasterInsuranceCompanyDto dto);
        Task<Result> UpdateAsync(int id, UpdateMasterInsuranceCompanyDto dto);
        Task<Result> DeleteAsync(int id);
        Task<Result> ToggleStatusAsync(int id);
    }

    public class MasterInsuranceCompaniesApiClient : BaseApiClient, IMasterInsuranceCompaniesApiClient
    {
        public MasterInsuranceCompaniesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<MasterInsuranceCompanyDto>>> GetAllAsync(string? search = null, bool? isActive = null)
        {
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");
                if (isActive.HasValue)
                    queryParams.Add($"isActive={isActive.Value}");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"api/master/insurance-companies{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<MasterInsuranceCompanyDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<List<MasterInsuranceCompanyDto>>>(_jsonOptions);
                return result ?? Result<List<MasterInsuranceCompanyDto>>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<List<MasterInsuranceCompanyDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<List<InsuranceCompanySelectDto>>> GetForSelectAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/master/insurance-companies/select");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<InsuranceCompanySelectDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<List<InsuranceCompanySelectDto>>>(_jsonOptions);
                return result ?? Result<List<InsuranceCompanySelectDto>>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<List<InsuranceCompanySelectDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<MasterInsuranceCompanyDto>> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/master/insurance-companies/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<MasterInsuranceCompanyDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<MasterInsuranceCompanyDto>>(_jsonOptions);
                return result ?? Result<MasterInsuranceCompanyDto>.Failure("Not found");
            }
            catch (Exception ex)
            {
                return Result<MasterInsuranceCompanyDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<MasterInsuranceCompanyDto>> CreateAsync(CreateMasterInsuranceCompanyDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/master/insurance-companies", dto, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<MasterInsuranceCompanyDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<MasterInsuranceCompanyDto>>(_jsonOptions);
                return result ?? Result<MasterInsuranceCompanyDto>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<MasterInsuranceCompanyDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> UpdateAsync(int id, UpdateMasterInsuranceCompanyDto dto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/master/insurance-companies/{id}", dto, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/master/insurance-companies/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> ToggleStatusAsync(int id)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"api/master/insurance-companies/{id}/toggle-status", null);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }
    }
}
