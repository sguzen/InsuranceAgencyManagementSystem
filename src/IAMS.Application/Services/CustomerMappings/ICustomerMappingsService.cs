using IAMS.Application.DTOs.CustomerMapping;

namespace IAMS.Application.Services.CustomerMappings
{
    public interface ICustomerMappingService
    {
        Task<List<CustomerMappingDto>> GetMappingsByCustomerIdAsync(int customerId);
        Task<List<CustomerMappingDto>> GetMappingsByCompanyIdAsync(int companyId);
        Task<CustomerMappingDto> CreateMappingAsync(CreateCustomerMappingDto mappingDto);
        Task UpdateMappingAsync(int id, UpdateCustomerMappingDto mappingDto);
        Task DeleteMappingAsync(int id);
        Task<string?> GetExternalCustomerIdAsync(int customerId, int companyId);
    }
}