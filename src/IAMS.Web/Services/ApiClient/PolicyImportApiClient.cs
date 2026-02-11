using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Models;
using System.Net.Http.Json;

namespace IAMS.Web.Services.ApiClient
{
    public interface IPolicyImportApiClient
    {
        Task<Result<List<ImportableCompanyDto>>> GetAvailableCompaniesAsync();
        Task<Result<ImportJobDto>> RequestImportAsync(ImportRequestDto request);
        Task<Result<ImportJobDto>> GetJobStatusAsync(int jobId);
        Task<Result<List<ImportJobDto>>> GetRecentJobsAsync(int? insuranceCompanyId = null);
        Task<Result> CancelJobAsync(int jobId);
        Task<Result<MySqlImportPreviewResult>> PreviewMySqlImportAsync(ImportRequestDto request);
    }

    public class PolicyImportApiClient : BaseApiClient, IPolicyImportApiClient
    {
        public PolicyImportApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<ImportableCompanyDto>>> GetAvailableCompaniesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/import/available-companies");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<List<ImportableCompanyDto>>.Failure($"API error: {response.StatusCode}", error);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<List<ImportableCompanyDto>>>(_jsonOptions);
                return result ?? Result<List<ImportableCompanyDto>>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<List<ImportableCompanyDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<ImportJobDto>> RequestImportAsync(ImportRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/import/request", request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<ImportJobDto>.Failure($"API error: {response.StatusCode}", error);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<ImportJobDto>>(_jsonOptions);
                return result ?? Result<ImportJobDto>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<ImportJobDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<ImportJobDto>> GetJobStatusAsync(int jobId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/import/jobs/{jobId}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<ImportJobDto>.Failure($"API error: {response.StatusCode}", error);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<ImportJobDto>>(_jsonOptions);
                return result ?? Result<ImportJobDto>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<ImportJobDto>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<List<ImportJobDto>>> GetRecentJobsAsync(int? insuranceCompanyId = null)
        {
            try
            {
                var url = "api/import/jobs";
                if (insuranceCompanyId.HasValue)
                {
                    url += $"?insuranceCompanyId={insuranceCompanyId}";
                }

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<List<ImportJobDto>>.Failure($"API error: {response.StatusCode}", error);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<List<ImportJobDto>>>(_jsonOptions);
                return result ?? Result<List<ImportJobDto>>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<List<ImportJobDto>>.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result> CancelJobAsync(int jobId)
        {
            try
            {
                var response = await _httpClient.PostAsync($"api/import/jobs/{jobId}/cancel", null);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result.Failure($"API error: {response.StatusCode}", error);
                }

                var result = await response.Content.ReadFromJsonAsync<Result>(_jsonOptions);
                return result ?? Result.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result.Failure($"Error: {ex.Message}");
            }
        }

        public async Task<Result<MySqlImportPreviewResult>> PreviewMySqlImportAsync(ImportRequestDto request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/import/preview-mysql", request, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Result<MySqlImportPreviewResult>.Failure($"API error: {response.StatusCode}", error);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<MySqlImportPreviewResult>>(_jsonOptions);
                return result ?? Result<MySqlImportPreviewResult>.Failure("Empty response");
            }
            catch (Exception ex)
            {
                return Result<MySqlImportPreviewResult>.Failure($"Error: {ex.Message}");
            }
        }
    }

    // DTOs (matching API)
    public class ImportableCompanyDto
    {
        public int MasterInsuranceCompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool HasCredentials { get; set; }
        public DateTime? LastImportDate { get; set; }
    }

    public class ImportRequestDto
    {
        public int MasterInsuranceCompanyId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ImportJobDto
    {
        public int Id { get; set; }
        public int InsuranceCompanyId { get; set; }
        public string? InsuranceCompanyName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TotalRecords { get; set; }
        public int ImportedRecords { get; set; }
        public int FailedRecords { get; set; }
        public int SkippedRecords { get; set; }
        public string? ErrorMessage { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public double? Duration { get; set; }
    }
}
