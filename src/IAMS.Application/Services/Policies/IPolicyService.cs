using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;

public interface IPolicyService
{
    Task<Result<PolicyDto>> GetPolicyByIdAsync(int id);
    Task<Result<PagedResult<PolicyDto>>> GetPoliciesAsync(PolicyQueryParams queryParams);
    Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto createPolicyDto);
    Task<Result<PolicyDto>> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto);
    Task<Result> DeletePolicyAsync(int id);
    Task<Result<PolicyDto>> GetPolicyByNumberAsync(string policyNumber);
    Task<Result<List<PolicyDto>>> GetPoliciesByCustomerAsync(int customerId);
    Task<Result<List<PolicyDto>>> GetActivePoliciesAsync();
    Task<Result<List<PolicyDto>>> GetExpiringPoliciesAsync(int daysAhead = 30);
    Task<Result<List<PolicyDto>>> GetExpiredPoliciesAsync();
    Task<Result<PolicyDto>> ActivatePolicyAsync(int id);
    Task<Result<PolicyDto>> CancelPolicyAsync(int id, string? reason = null);
    Task<Result<PolicyDto>> SuspendPolicyAsync(int id, string? reason = null);
    Task<Result<PolicyDto>> RenewPolicyAsync(int id, CreatePolicyDto renewalDto);
}