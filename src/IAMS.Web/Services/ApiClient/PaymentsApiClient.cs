using IAMS.Shared.DTOs.Payment;
using IAMS.Application.Models;
using IAMS.Domain.Enums;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IPaymentsApiClient
    {
        Task<PagedResult<PolicyPaymentDto>> GetPaymentsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<Result<PagedResult<PolicyPaymentDto>>> GetPaymentsPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        Task<PolicyPaymentDto?> GetPaymentByIdAsync(int id);
        Task<List<PolicyPaymentDto>> GetPaymentsByPolicyAsync(int policyId);
        Task<Result<List<PolicyPaymentDto>>> GetPaymentsByPolicyIdAsync(int policyId);
        Task<decimal> GetTotalPaymentsByPolicyAsync(int policyId);
        Task<Result<decimal>> GetTotalPaymentsByPolicyIdAsync(int policyId);
        Task<List<PolicyPaymentDto>> GetOverduePaymentsAsync();
        Task<Result<PolicyPaymentDto>> CreatePaymentAsync(CreatePolicyPaymentDto paymentDto);
        Task<Result<PolicyPaymentDto>> CreateAsync(CreatePolicyPaymentDto paymentDto);
        Task<Result> UpdateAsync(int id, UpdatePolicyPaymentDto paymentDto);
        Task<Result> UpdatePaymentStatusAsync(int id, PaymentStatus status);
        Task<Result> DeletePaymentAsync(int id);
        Task<Result> DeleteAsync(int id);
        Task<List<CustomerOutstandingBalanceDto>> GetCustomersWithOutstandingBalanceAsync();
        Task<List<PolicyPaymentDto>> GetPaymentsDueThisMonthAsync();
    }

    public class PaymentsApiClient : BaseApiClient, IPaymentsApiClient
    {
        public PaymentsApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<PagedResult<PolicyPaymentDto>> GetPaymentsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
        {
            var queryString = $"?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(searchTerm))
                queryString += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";

            var result = await GetAsync<PagedResult<PolicyPaymentDto>>($"api/payments{queryString}");
            return result.Data ?? new PagedResult<PolicyPaymentDto>();
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

        public async Task<List<PolicyPaymentDto>> GetPaymentsByPolicyAsync(int policyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/payments/policy/{policyId}");
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

        public async Task<decimal> GetTotalPaymentsByPolicyAsync(int policyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/payments/policy/{policyId}/total");
                if (!response.IsSuccessStatusCode)
                    return 0;

                return await response.Content.ReadFromJsonAsync<decimal>(_jsonOptions);
            }
            catch
            {
                return 0;
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

        public async Task<Result<PolicyPaymentDto>> CreatePaymentAsync(CreatePolicyPaymentDto paymentDto)
        {
            return await PostAsync<PolicyPaymentDto>("api/payments", paymentDto);
        }

        public async Task<Result> UpdatePaymentStatusAsync(int id, PaymentStatus status)
        {
            return await PatchAsync($"api/payments/{id}/status", new { Status = status });
        }

        public async Task<Result> DeletePaymentAsync(int id)
        {
            return await base.DeleteAsync($"api/payments/{id}");
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

        public async Task<Result<PolicyPaymentDto>> CreateAsync(CreatePolicyPaymentDto paymentDto)
        {
            return await PostAsync<PolicyPaymentDto>("api/payments", paymentDto);
        }

        public async Task<Result> UpdateAsync(int id, UpdatePolicyPaymentDto paymentDto)
        {
            return await PutAsync($"api/payments/{id}", paymentDto);
        }

        public async Task<List<CustomerOutstandingBalanceDto>> GetCustomersWithOutstandingBalanceAsync()
        {
            var result = await GetAsync<List<CustomerOutstandingBalanceDto>>("api/payments/outstanding-balances");
            return result.Data ?? new List<CustomerOutstandingBalanceDto>();
        }

        public async Task<List<PolicyPaymentDto>> GetPaymentsDueThisMonthAsync()
        {
            var result = await GetAsync<List<PolicyPaymentDto>>("api/payments/due-this-month");
            return result.Data ?? new List<PolicyPaymentDto>();
        }
    }
}
