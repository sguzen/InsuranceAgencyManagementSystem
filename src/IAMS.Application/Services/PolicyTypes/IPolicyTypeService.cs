using IAMS.Application.DTOs.PolicyType;

namespace IAMS.Application.Services.PolicyTypes
{
    public interface IPolicyTypeService
    {
        Task<List<PolicyTypeDto>> GetAllAsync();
        Task<PolicyTypeDto?> GetByIdAsync(int id);
        Task<PolicyTypeDto> CreateAsync(CreatePolicyTypeDto policyTypeDto);
        Task UpdateAsync(int id, UpdatePolicyTypeDto policyTypeDto);
        Task DeleteAsync(int id);
        Task<List<PolicyTypeDto>> GetActiveTypesAsync();
    }
}