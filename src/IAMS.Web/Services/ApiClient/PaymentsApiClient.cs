using IAMS.Shared.DTOs.Payment;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IPaymentsApiClient
    {
        Task<Result<PagedResult<PolicyPaymentDto>>> GetPaymentsPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<PolicyPaymentDto?> GetPaymentByIdAsync(int id);
        Task<Result<List<PolicyPaymentDto>>> GetPaymentsByPolicyIdAsync(int policyId);
        Task<Result<decimal>> GetTotalPaymentsByPolicyIdAsync(int policyId);
        Task<List<PolicyPaymentDto>> GetOverduePaymentsAsync();
        Task<List<PolicyPaymentDto>> GetPaymentsDueThisMonthAsync(int? limit = null);
        Task<Result<PolicyPaymentDto>> CreateAsync(CreatePolicyPaymentDto paymentDto);
        Task<Result> UpdateAsync(int id, UpdatePolicyPaymentDto paymentDto);
        Task<Result> UpdatePaymentStatusAsync(int id, PaymentStatus status);
        Task<Result> DeleteAsync(int id);
        Task<List<CustomerOutstandingBalanceDto>> GetCustomersWithOutstandingBalanceAsync(int? limit = null);
    }

    public class PaymentsApiClient : BaseApiClient, IPaymentsApiClient
    {
        public PaymentsApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<PolicyPaymentDto?> GetPaymentByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/payments/{id}");
                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<PolicyPaymentDto>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PolicyPaymentDto>> GetOverduePaymentsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/payments/overdue");
                if (!response.IsSuccessStatusCode)
                    return new List<PolicyPaymentDto>();

                return await response.Content.ReadFromJsonAsync<List<PolicyPaymentDto>>(_jsonOptions)
                    ?? new List<PolicyPaymentDto>();
            }
            catch
            {
                return new List<PolicyPaymentDto>();
            }
        }

        public async Task<Result> UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            return await PatchAsync($"api/payments/{id}/status", new { Status = status });
        }

        public async Task<Result> DeleteAsync(int id)
        {
            return await base.DeleteAsync($"api/payments/{id}");
        }

        public async Task<Result<PagedResult<PolicyPaymentDto>>> GetPaymentsPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            return await GetAsync<PagedResult<PolicyPaymentDto>>($"api/payments{queryString}");
        }

        public async Task<Result<List<PolicyPaymentDto>>> GetPaymentsByPolicyIdAsync(int policyId)
        {
            return await GetAsync<List<PolicyPaymentDto>>($"api/payments/policy/{policyId}");
        }

        public async Task<Result<decimal>> GetTotalPaymentsByPolicyIdAsync(int policyId)
        {
            return await GetAsync<decimal>($"api/payments/policy/{policyId}/total");
        }

        public async Task<List<PolicyPaymentDto>> GetPaymentsDueThisMonthAsync(int? limit = null)
        {
            try
            {
                var url = "api/payments/due-this-month";
                if (limit.HasValue && limit.Value > 0)
                {
                    url += $"?limit={limit.Value}";
                }

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<PolicyPaymentDto>();

                return await response.Content.ReadFromJsonAsync<List<PolicyPaymentDto>>(_jsonOptions)
                    ?? new List<PolicyPaymentDto>();
            }
            catch
            {
                return new List<PolicyPaymentDto>();
            }
        }

        public async Task<Result<PolicyPaymentDto>> CreateAsync(CreatePolicyPaymentDto paymentDto)
        {
            return await PostAsync<PolicyPaymentDto>("api/payments", paymentDto);
        }

        public async Task<Result> UpdateAsync(int id, UpdatePolicyPaymentDto paymentDto)
        {
            return await PutAsync($"api/payments/{id}", paymentDto);
        }

        public async Task<List<CustomerOutstandingBalanceDto>> GetCustomersWithOutstandingBalanceAsync(int? limit = null)
        {
            var url = "api/payments/outstanding-balances";
            if (limit.HasValue)
            {
                url += $"?limit={limit.Value}";
            }
            var result = await GetAsync<List<CustomerOutstandingBalanceDto>>(url);
            return result.Data ?? new List<CustomerOutstandingBalanceDto>();
        }
    }
}
