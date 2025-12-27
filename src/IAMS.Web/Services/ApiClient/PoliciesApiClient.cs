using IAMS.Shared.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;

namespace IAMS.Web.Services.ApiClient
{
    public interface IPoliciesApiClient
    {
        Task<Result<PagedResult<PolicyDto>>> GetPoliciesAsync(PolicyQueryParams queryParams);
        Task<Result<PolicyDto>> GetPolicyByIdAsync(int id);
        Task<Result<PolicyDto>> GetPolicyByNumberAsync(string policyNumber);
        Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto policyDto);
        Task<Result<PolicyDto>> UpdatePolicyAsync(int id, UpdatePolicyDto policyDto);
        Task<Result> DeletePolicyAsync(int id);
        Task<Result<PolicyDto>> ActivatePolicyAsync(int id);
        Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason);
        Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason);
        Task<Result<PolicyDto>> ReactivatePolicyAsync(int id);
        Task<Result<PolicyDto>> RenewPolicyAsync(int id, DateTime startDate, DateTime endDate, decimal premiumAmount);
        Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30);
        Task<Result<List<PolicyDto>>> GetPoliciesByCustomerAsync(int customerId);
        Task<Result<List<PolicyDto>>> GetEndorsementsByPolicyIdAsync(int policyId);
        Task<Result<PolicyStatisticsDto>> GetStatisticsAsync();
        Task<Result<int>> GetTotalPoliciesCountAsync();
        Task<Result<int>> GetExpiringPoliciesCountAsync(int daysAhead = 30);
        Task<Result<Dictionary<string, decimal>>> GetMonthlyRevenueByCurrencyAsync();
        Task<Result<decimal>> GetMonthlyRevenueAsync();
        Task<Result<List<PolicyImportPreviewDto>>> ParsePolicyImportAsync(Stream fileStream, string fileName, int insuranceCompanyId);
        Task<Result<PolicyImportResultDto>> ImportPoliciesWithMappingAsync(List<PolicyImportPreviewDto> mappedPolicies, int insuranceCompanyId);
        Task<Result<PolicyImportResultDto>> ImportPoliciesAsync(Stream fileStream, string fileName, int insuranceCompanyId);
    }

    public class PoliciesApiClient : BaseApiClient, IPoliciesApiClient
    {
        private readonly IPolicyFormattingService _policyFormattingService;

        public PoliciesApiClient(HttpClient httpClient, IPolicyFormattingService policyFormattingService) : base(httpClient)
        {
            _policyFormattingService = policyFormattingService;
        }

        public async Task<Result<PagedResult<PolicyDto>>> GetPoliciesAsync(PolicyQueryParams queryParams)
        {
            var queryString = $"?pageNumber={queryParams.PageNumber}&pageSize={queryParams.PageSize}";
            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(queryParams.SearchTerm)}";
            if (queryParams.CustomerId.HasValue)
                queryString += $"&customerId={queryParams.CustomerId}";
            if (queryParams.InsuranceCompanyId.HasValue)
                queryString += $"&insuranceCompanyId={queryParams.InsuranceCompanyId}";
            if (queryParams.PolicyTypeId.HasValue)
                queryString += $"&policyTypeId={queryParams.PolicyTypeId}";
            queryString += $"&status={queryParams.Status}";

            var result = await GetAsync<PagedResult<PolicyDto>>($"api/policies{queryString}");
            if (result.IsSuccess && result.Data?.Items != null)
            {
                _policyFormattingService.FormatPolicyNumbers(result.Data.Items);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> GetPolicyByIdAsync(int id)
        {
            var result = await GetAsync<PolicyDto>($"api/policies/{id}");
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> GetPolicyByNumberAsync(string policyNumber)
        {
            var result = await GetAsync<PolicyDto>($"api/policies/by-number/{Uri.EscapeDataString(policyNumber)}");
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto policyDto)
        {
            var result = await PostAsync<PolicyDto>("api/policies", policyDto);
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> UpdatePolicyAsync(int id, UpdatePolicyDto policyDto)
        {
            var result = await PutAsync<PolicyDto>($"api/policies/{id}", policyDto);
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result> DeletePolicyAsync(int id)
        {
            return await DeleteAsync($"api/policies/{id}");
        }

        public async Task<Result<PolicyDto>> ActivatePolicyAsync(int id)
        {
            var result = await PostAsync<PolicyDto>($"api/policies/{id}/activate", null);
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason)
        {
            var result = await PostAsync<PolicyDto>($"api/policies/{id}/cancel", new { Reason = reason });
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason)
        {
            var result = await PostAsync<PolicyDto>($"api/policies/{id}/suspend", new { Reason = reason });
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> ReactivatePolicyAsync(int id)
        {
            var result = await PostAsync<PolicyDto>($"api/policies/{id}/reactivate", null);
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyDto>> RenewPolicyAsync(int id, DateTime startDate, DateTime endDate, decimal premiumAmount)
        {
            var result = await PostAsync<PolicyDto>($"api/policies/{id}/renew", new
            {
                StartDate = startDate,
                EndDate = endDate,
                PremiumAmount = premiumAmount
            });
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumber(result.Data);
            }
            return result;
        }

        public async Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30)
        {
            var result = await GetAsync<List<PolicyDto>>($"api/policies/expiring?daysAhead={daysAhead}");
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumbers(result.Data);
            }
            return result;
        }

        public async Task<Result<List<PolicyDto>>> GetPoliciesByCustomerAsync(int customerId)
        {
            var result = await GetAsync<List<PolicyDto>>($"api/policies/customer/{customerId}");
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumbers(result.Data);
            }
            return result;
        }

        public async Task<Result<List<PolicyDto>>> GetEndorsementsByPolicyIdAsync(int policyId)
        {
            var result = await GetAsync<List<PolicyDto>>($"api/policies/{policyId}/endorsements");
            if (result.IsSuccess && result.Data != null)
            {
                _policyFormattingService.FormatPolicyNumbers(result.Data);
            }
            return result;
        }

        public async Task<Result<PolicyStatisticsDto>> GetStatisticsAsync()
        {
            return await GetAsync<PolicyStatisticsDto>("api/policies/statistics");
        }

        public async Task<Result<int>> GetTotalPoliciesCountAsync()
        {
            return await GetAsync<int>("api/policies/count");
        }

        public async Task<Result<int>> GetExpiringPoliciesCountAsync(int daysAhead = 30)
        {
            return await GetAsync<int>($"api/policies/expiring/count?daysAhead={daysAhead}");
        }

        public async Task<Result<Dictionary<string, decimal>>> GetMonthlyRevenueByCurrencyAsync()
        {
            return await GetAsync<Dictionary<string, decimal>>("api/policies/revenue/monthly-by-currency");
        }

        public async Task<Result<decimal>> GetMonthlyRevenueAsync()
        {
            return await GetAsync<decimal>("api/policies/revenue/monthly");
        }

        public async Task<Result<List<PolicyImportPreviewDto>>> ParsePolicyImportAsync(Stream fileStream, string fileName, int insuranceCompanyId)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(insuranceCompanyId.ToString()), "insuranceCompanyId");

            try
            {
                var response = await _httpClient.PostAsync("api/policies/import/parse", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<List<PolicyImportPreviewDto>>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<List<PolicyImportPreviewDto>>>(_jsonOptions);
                return result ?? Result<List<PolicyImportPreviewDto>>.Failure("Empty response from API", new List<string>());
            }
            catch (Exception ex)
            {
                return Result<List<PolicyImportPreviewDto>>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }

        public async Task<Result<PolicyImportResultDto>> ImportPoliciesWithMappingAsync(List<PolicyImportPreviewDto> mappedPolicies, int insuranceCompanyId)
        {
            var request = new
            {
                MappedPolicies = mappedPolicies,
                InsuranceCompanyId = insuranceCompanyId
            };

            return await PostAsync<PolicyImportResultDto>("api/policies/import/with-mapping", request);
        }

        public async Task<Result<PolicyImportResultDto>> ImportPoliciesAsync(Stream fileStream, string fileName, int insuranceCompanyId)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);
            content.Add(new StringContent(insuranceCompanyId.ToString()), "insuranceCompanyId");

            try
            {
                var response = await _httpClient.PostAsync("api/policies/import", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Result<PolicyImportResultDto>.Failure($"API request failed: {response.StatusCode}", errorContent);
                }

                var result = await response.Content.ReadFromJsonAsync<Result<PolicyImportResultDto>>(_jsonOptions);
                return result ?? Result<PolicyImportResultDto>.Failure("Empty response from API", new List<string>());
            }
            catch (Exception ex)
            {
                return Result<PolicyImportResultDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }
    }
}
