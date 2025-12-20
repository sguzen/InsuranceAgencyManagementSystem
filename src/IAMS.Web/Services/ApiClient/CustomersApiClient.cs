using IAMS.Application.DTOs.Customer;
using IAMS.Application.DTOs.Payment;
using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using IAMS.Shared.QueryParams;

namespace IAMS.Web.Services.ApiClient
{
    public interface ICustomersApiClient
    {
        Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(CustomerQueryParams queryParams);
        Task<Result<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto customerDto);
        Task<Result<CustomerDto>> UpdateCustomerAsync(int id, UpdateCustomerDto customerDto);
        Task<Result> DeleteCustomerAsync(int id);
        Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync();
        Task<Result<CustomerBalanceDto>> GetCustomerBalanceAsync(int id);
        Task<Result<List<CustomerPaymentDto>>> GetCustomerPaymentsAsync(int id);
        Task<Result<CustomerPaymentDto>> CreateCustomerPaymentAsync(int id, CreateCustomerPaymentDto paymentDto);
        Task<Result<CustomerMultiCurrencyStatementDto>> GetCustomerStatementAsync(int id, DateTime? startDate, DateTime? endDate, int? currencyId);
    }

    public class CustomersApiClient : BaseApiClient, ICustomersApiClient
    {
        public CustomersApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(CustomerQueryParams queryParams)
        {
            var query = $"?pageNumber={queryParams.PageNumber}&pageSize={queryParams.PageSize}";
            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
                query += $"&searchTerm={Uri.EscapeDataString(queryParams.SearchTerm)}";
            if (queryParams.Status.HasValue)
                query += $"&status={queryParams.Status}";
            if (queryParams.CreatedFrom.HasValue)
                query += $"&createdFrom={queryParams.CreatedFrom:yyyy-MM-dd}";

            return await GetAsync<PagedResult<CustomerDto>>($"api/customers{query}");
        }

        public async Task<Result<CustomerDto>> GetCustomerByIdAsync(int id)
        {
            return await GetAsync<CustomerDto>($"api/customers/{id}");
        }

        public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto customerDto)
        {
            return await PostAsync<CustomerDto>("api/customers", customerDto);
        }

        public async Task<Result<CustomerDto>> UpdateCustomerAsync(int id, UpdateCustomerDto customerDto)
        {
            return await PutAsync<CustomerDto>($"api/customers/{id}", customerDto);
        }

        public async Task<Result> DeleteCustomerAsync(int id)
        {
            return await DeleteAsync($"api/customers/{id}");
        }

        public async Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync()
        {
            return await GetAsync<CustomerStatisticsDto>("api/customers/statistics");
        }

        public async Task<Result<CustomerBalanceDto>> GetCustomerBalanceAsync(int id)
        {
            return await GetAsync<CustomerBalanceDto>($"api/customers/{id}/balance");
        }

        public async Task<Result<List<CustomerPaymentDto>>> GetCustomerPaymentsAsync(int id)
        {
            return await GetAsync<List<CustomerPaymentDto>>($"api/customers/{id}/payments");
        }

        public async Task<Result<CustomerPaymentDto>> CreateCustomerPaymentAsync(int id, CreateCustomerPaymentDto paymentDto)
        {
            return await PostAsync<CustomerPaymentDto>($"api/customers/{id}/payments", paymentDto);
        }

        public async Task<Result<CustomerMultiCurrencyStatementDto>> GetCustomerStatementAsync(
            int id, DateTime? startDate, DateTime? endDate, int? currencyId)
        {
            var query = "";
            if (startDate.HasValue)
                query += $"?startDate={startDate:yyyy-MM-dd}";
            if (endDate.HasValue)
                query += $"{(query.Length > 0 ? "&" : "?")}endDate={endDate:yyyy-MM-dd}";
            if (currencyId.HasValue)
                query += $"{(query.Length > 0 ? "&" : "?")}currencyId={currencyId}";

            return await GetAsync<CustomerMultiCurrencyStatementDto>($"api/customers/{id}/statement{query}");
        }
    }
}
