using IAMS.Shared.QueryParams;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Shared.Models;
using IAMS.Domain.Enums;
using MediatR;

namespace IAMS.Application.Services.Customers
{
    public interface ICustomerService
    {
        Task<Result<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(CustomerQueryParams queryParams);
        Task<Result<CustomerDto>> CreateCustomerAsync(CreateOrUpdateCustomerDto createCustomerDto);
        Task<Result<CustomerDto>> UpdateCustomerAsync(int id, CreateOrUpdateCustomerDto updateCustomerDto);
        Task<Result> DeleteCustomerAsync(int id);
        Task<Result<CustomerDto>> GetCustomerByIdentificationNoAsync(string tcNo);
        Task<Result<CustomerDto>> GetCustomerByEmailAsync(string email);
        Task<Result<CustomerDto>> GetCustomerByCodeAsync(string customerCode);
        Task<Result<List<CustomerDto>>> GetCustomersWithActivePoliciesAsync();
        Task<Result<bool>> ValidateIdentificationNoAsync(string tcNo, int? excludeCustomerId = null);
        Task<Result<bool>> ValidateEmailAsync(string email, int? excludeCustomerId = null);
        // Dashboard statistics methods
        Task<Result<int>> GetTotalCustomersCountAsync();
        Task<Result<List<CustomerDto>>> GetRecentCustomersAsync(int count = 5);
        Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync();
        Task<Result<List<CustomerDto>>> GetTopCustomersByPolicyCountAsync(int count = 10);
        Task<Result<Dictionary<CustomerStatus, int>>> GetCustomersByStatusAsync();
        Task<Result<Dictionary<string, int>>> GetCustomersCreatedByMonthAsync(int months = 12);
    }
}