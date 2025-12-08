using IAMS.Shared.QueryParams;
using IAMS.Shared.Models;
using IAMS.Domain.Enums;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

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
    Task ProcessExpiringPoliciesAsync();

    // Dashboard statistics methods
    Task<Result<int>> GetTotalPoliciesCountAsync();
    Task<Result<int>> GetExpiringPoliciesCountAsync(int daysAhead = 30);
    Task<Result<decimal>> GetMonthlyRevenueAsync();
    Task<Result<Dictionary<string, decimal>>> GetMonthlyRevenueByCurrencyAsync();
    Task<Result<PolicyStatisticsDto>> GetPolicyStatisticsAsync();
    Task<Result<List<PolicyDto>>> GetRecentPoliciesAsync(int count = 5);
    Task<Result<Dictionary<PolicyStatus, int>>> GetPoliciesByStatusAsync();
    Task<Result<Dictionary<string, decimal>>> GetRevenueByMonthAsync(int months = 12);
    Task<Result<List<PolicyDto>>> GetTopPoliciesByPremiumAsync(int count = 10);

    // Endorsement methods
    Task<Result<List<PolicyDto>>> GetEndorsementsByPolicyIdAsync(int policyId);
}