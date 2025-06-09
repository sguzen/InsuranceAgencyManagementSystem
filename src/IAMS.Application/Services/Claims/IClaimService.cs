using IAMS.Application.DTOs.Claim;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Services.Claims
{
    public interface IClaimService
    {
        Task<List<PolicyClaimDto>> GetClaimsByPolicyIdAsync(int policyId);
        Task<PolicyClaimDto?> GetByIdAsync(int id);
        Task<PolicyClaimDto> CreateAsync(CreatePolicyClaimDto claimDto);
        Task UpdateClaimStatusAsync(int id, ClaimStatus status);
        Task DeleteAsync(int id);
        Task<List<PolicyClaimDto>> GetClaimsByStatusAsync(ClaimStatus status);
        Task<PagedResult<PolicyClaimDto>> GetClaimsPagedAsync(int pageNumber, int pageSize, string searchTerm = null);
        Task<decimal> GetTotalClaimAmountByPolicyIdAsync(int policyId);
    }
}