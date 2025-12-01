using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for querying and searching policies
    /// </summary>
    public interface IPolicyQueryService
    {
        // Paged queries
        Task<PagedResult<Policy>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<PagedResult<Policy>> GetPoliciesByStatusPagedAsync(PolicyStatus status, int pageNumber, int pageSize);

        // Status-based queries
        Task<List<Policy>> GetActivePoliciesAsync();
        Task<List<Policy>> GetPoliciesByStatusAsync(PolicyStatus status);
        Task<List<Policy>> GetExpiringPoliciesAsync(int daysAhead);
        Task<List<Policy>> GetExpiredPoliciesAsync();
        Task<List<Policy>> GetRecentPoliciesAsync(int count);
        Task<List<Policy>> GetTopPoliciesByPremiumAsync(int count);

        // Advanced queries
        Task<List<Policy>> GetPoliciesExpiringInDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Policy>> SearchPoliciesAsync(string searchTerm);
        Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId);
        Task<List<Policy>> GetPoliciesByPolicyTypeAsync(int policyTypeId);
        Task<List<Policy>> GetByVehicleIdAsync(int vehicleId);

        // Endorsement queries
        Task<List<Policy>> GetEndorsementsByOriginalPolicyIdAsync(int originalPolicyId);
        Task<int> GetEndorsementCountAsync(int originalPolicyId);

        // Business rule validation
        Task<bool> HasOverduePaymentsAsync(int policyId);
        Task<bool> CanBeCancelledAsync(int policyId);
        Task<bool> CanBeRenewedAsync(int policyId);
    }
}
