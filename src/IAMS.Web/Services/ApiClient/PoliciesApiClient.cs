using IAMS.Application.DTOs.Policy;
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
        Task<Result<PolicyStatisticsDto>> GetStatisticsAsync();
        Task<Result<PolicyImportResultDto>> ImportPoliciesAsync(Stream fileStream, string fileName, int insuranceCompanyId);
    }

    public class PoliciesApiClient : BaseApiClient, IPoliciesApiClient
    {
        public PoliciesApiClient(HttpClient httpClient) : base(httpClient)
        {
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

            return await GetAsync<PagedResult<PolicyDto>>($"api/policies{queryString}");
        }

        public async Task<Result<PolicyDto>> GetPolicyByIdAsync(int id)
        {
            return await GetAsync<PolicyDto>($"api/policies/{id}");
        }

        public async Task<Result<PolicyDto>> GetPolicyByNumberAsync(string policyNumber)
        {
            return await GetAsync<PolicyDto>($"api/policies/by-number/{Uri.EscapeDataString(policyNumber)}");
        }

        public async Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto policyDto)
        {
            return await PostAsync<PolicyDto>("api/policies", policyDto);
        }

        public async Task<Result<PolicyDto>> UpdatePolicyAsync(int id, UpdatePolicyDto policyDto)
        {
            return await PutAsync<PolicyDto>($"api/policies/{id}", policyDto);
        }

        public async Task<Result> DeletePolicyAsync(int id)
        {
            return await DeleteAsync($"api/policies/{id}");
        }

        public async Task<Result<PolicyDto>> ActivatePolicyAsync(int id)
        {
            return await PostAsync<PolicyDto>($"api/policies/{id}/activate", null);
        }

        public async Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason)
        {
            return await PostAsync<PolicyDto>($"api/policies/{id}/cancel", new { Reason = reason });
        }

        public async Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason)
        {
            return await PostAsync<PolicyDto>($"api/policies/{id}/suspend", new { Reason = reason });
        }

        public async Task<Result<PolicyDto>> ReactivatePolicyAsync(int id)
        {
            return await PostAsync<PolicyDto>($"api/policies/{id}/reactivate", null);
        }

        public async Task<Result<PolicyDto>> RenewPolicyAsync(int id, DateTime startDate, DateTime endDate, decimal premiumAmount)
        {
            return await PostAsync<PolicyDto>($"api/policies/{id}/renew", new
            {
                StartDate = startDate,
                EndDate = endDate,
                PremiumAmount = premiumAmount
            });
        }

        public async Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30)
        {
            return await GetAsync<List<PolicyDto>>($"api/policies/expiring?daysAhead={daysAhead}");
        }

        public async Task<Result<List<PolicyDto>>> GetPoliciesByCustomerAsync(int customerId)
        {
            return await GetAsync<List<PolicyDto>>($"api/policies/customer/{customerId}");
        }

        public async Task<Result<PolicyStatisticsDto>> GetStatisticsAsync()
        {
            return await GetAsync<PolicyStatisticsDto>("api/policies/statistics");
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
                return result ?? Result<PolicyImportResultDto>.Failure("Empty response from API");
            }
            catch (Exception ex)
            {
                return Result<PolicyImportResultDto>.Failure($"API call failed: {ex.Message}", ex.ToString());
            }
        }
    }
}
