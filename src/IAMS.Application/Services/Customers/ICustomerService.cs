using IAMS.Application.DTOs.Customer;
using IAMS.Application.Features.Customers.Commands.CreateCustomer;
using IAMS.Application.Features.Customers.Commands.DeleteCustomer;
using IAMS.Application.Features.Customers.Queries.GetCustomers;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Services.Customers
{
    public interface ICustomerService
    {
        Task<Result<CustomerDto>> GetCustomerByIdAsync(int id);
        Task<Result<PagedResult<CustomerDto>>> GetCustomersAsync(CustomerQueryParams queryParams);
        Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<Result<CustomerDto>> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task<r> DeleteCustomerAsync(int id);
        Task<Result<CustomerDto>> GetCustomerByTcNoAsync(string tcNo);
        Task<Result<CustomerDto>> GetCustomerByEmailAsync(string email);
        Task<Result<CustomerDto>> GetCustomerByCodeAsync(string customerCode);
        Task<Result<List<CustomerDto>>> GetCustomersWithActivePoliciesAsync();
        Task<Result<bool>> ValidateTcNoAsync(string tcNo, int? excludeCustomerId = null);
        Task<Result<bool>> ValidateEmailAsync(string email, int? excludeCustomerId = null);
    }
}