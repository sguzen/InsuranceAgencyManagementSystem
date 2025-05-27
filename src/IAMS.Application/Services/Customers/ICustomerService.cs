using IAMS.Application.DTOs.Customer;

namespace IAMS.Application.Services.Customers
{
    public interface ICustomerService
    {
        Task<List<CustomerDto>> GetAllAsync();
        Task<CustomerDto?> GetByIdAsync(int id);
        Task<CustomerDto> CreateAsync(CreateCustomerDto customerDto);
        Task UpdateAsync(int id, UpdateCustomerDto customerDto);
        Task DeleteAsync(int id);
    }
}