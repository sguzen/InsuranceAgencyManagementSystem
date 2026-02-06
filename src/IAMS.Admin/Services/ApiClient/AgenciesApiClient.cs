using IAMS.Shared.DTOs.Agency;
using IAMS.Shared.Models;
using System.Net.Http.Json;

namespace IAMS.Admin.Services.ApiClient
{
    public interface IAgenciesApiClient
    {
        Task<Result<List<AgencyDto>>> GetAgenciesAsync(int page = 1, int pageSize = 50, string? search = null);
        Task<Result<AgencyDto>> GetAgencyByIdAsync(int agencyId);
        Task<Result<AgencyDto>> CreateAgencyAsync(CreateAgencyDto createAgencyDto);
        Task<Result> UpdateAgencyAsync(int agencyId, UpdateAgencyDto updateAgencyDto);
        Task<Result> DeleteAgencyAsync(int agencyId);
        Task<Result> ActivateAgencyAsync(int agencyId);
        Task<Result> SuspendAgencyAsync(int agencyId);
    }

    public class AgenciesApiClient : BaseApiClient, IAgenciesApiClient
    {
        public AgenciesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<AgencyDto>>> GetAgenciesAsync(int page = 1, int pageSize = 50, string? search = null)
        {
            var queryString = $"?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(search))
                queryString += $"&search={Uri.EscapeDataString(search)}";

            try
            {
                var response = await _httpClient.GetAsync($"api/agencies{queryString}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<AgencyDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<AgenciesResponse>(_jsonOptions);
                if (result != null)
                {
                    return Result<List<AgencyDto>>.Success(result.Agencies, "Agencies loaded");
                }

                return Result<List<AgencyDto>>.Failure("Empty response", (List<string>?)null);
            }
            catch (Exception ex)
            {
                return Result<List<AgencyDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<AgencyDto>> GetAgencyByIdAsync(int agencyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/agencies/{agencyId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<AgencyDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var agency = await response.Content.ReadFromJsonAsync<AgencyDto>(_jsonOptions);
                if (agency != null)
                {
                    return Result<AgencyDto>.Success(agency);
                }

                return Result<AgencyDto>.Failure("Agency not found", (List<string>?)null);
            }
            catch (Exception ex)
            {
                return Result<AgencyDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<AgencyDto>> CreateAgencyAsync(CreateAgencyDto createAgencyDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/agencies", createAgencyDto, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<AgencyDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var agency = await response.Content.ReadFromJsonAsync<AgencyDto>(_jsonOptions);
                return agency != null
                    ? Result<AgencyDto>.Success(agency, "Acenta başarıyla oluşturuldu")
                    : Result<AgencyDto>.Success("Acenta başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                return Result<AgencyDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> UpdateAgencyAsync(int agencyId, UpdateAgencyDto updateAgencyDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/agencies/{agencyId}", updateAgencyDto, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Acenta başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> DeleteAgencyAsync(int agencyId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/agencies/{agencyId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Acenta askıya alındı");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> ActivateAgencyAsync(int agencyId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/agencies/{agencyId}/activate", new { }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Acenta aktifleştirildi");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result> SuspendAgencyAsync(int agencyId)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/agencies/{agencyId}/suspend", new { }, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                return Result.Success("Acenta askıya alındı");
            }
            catch (Exception ex)
            {
                return Result.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        private class AgenciesResponse
        {
            public List<AgencyDto> Agencies { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
