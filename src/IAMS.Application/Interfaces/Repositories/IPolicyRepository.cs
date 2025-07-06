using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        // Basic CRUD operations
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber);
        Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<bool> PolicyNumberExistsAsync(string policyNumber, int tenantId, int? excludePolicyId = null);

        // Paged queries
        Task<PagedResult<Policy>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<PagedResult<Policy>> GetPoliciesByStatusPagedAsync(PolicyStatus status, int pageNumber, int pageSize);

        // Status-based queries
        Task<List<Policy>> GetActivePoliciesAsync(int tenantId);
        Task<List<Policy>> GetPoliciesByStatusAsync(PolicyStatus status, int tenantId);
        Task<List<Policy>> GetExpiringPoliciesAsync(int daysAhead, int tenantId);
        Task<List<Policy>> GetExpiredPoliciesAsync(int tenantId);
        Task<List<Policy>> GetRecentPoliciesAsync(int count, int tenantId);
        Task<List<Policy>> GetTopPoliciesByPremiumAsync(int count, int tenantId);

        // Count queries
        Task<int> GetPolicyCountAsync(int tenantId);
        Task<int> GetActivePolicyCountAsync(int tenantId);
        Task<int> GetExpiringPolicyCountAsync(int daysAhead, int tenantId);
        Task<int> GetExpiredPolicyCountAsync(int tenantId);
        Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync(int tenantId);

        // Revenue queries
        Task<decimal> GetMonthlyRevenueAsync(int tenantId, DateTime? month = null);
        Task<decimal> GetYearlyRevenueAsync(int tenantId, int? year = null);
        Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int tenantId, int months = 12);
        Task<decimal> GetTotalPremiumByCustomerAsync(int customerId, int tenantId);

        // Business rule validation
        Task<bool> HasOverduePaymentsAsync(int policyId);
        Task<bool> CanBeCancelledAsync(int policyId);
        Task<bool> CanBeRenewedAsync(int policyId);

        // Advanced queries
        Task<List<Policy>> GetPoliciesExpiringInDateRangeAsync(DateTime startDate, DateTime endDate, int tenantId);
        Task<List<Policy>> SearchPoliciesAsync(string searchTerm, int tenantId);
        Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId, int tenantId);
        Task<List<Policy>> GetPoliciesByPolicyTypeAsync(int policyTypeId, int tenantId);

        // Reporting queries
        Task<List<Policy>> GetPoliciesForReportAsync(DateTime? startDate, DateTime? endDate, PolicyStatus? status, int tenantId);
        Task<Dictionary<string, object>> GetPolicyAnalyticsAsync(int tenantId);

        // Integration support
        Task<List<Policy>> GetPoliciesModifiedAfterAsync(DateTime modifiedDate, int tenantId);
        Task<Policy?> GetPolicyByExternalIdAsync(string externalId, int insuranceCompanyId, int tenantId);
        Task UpdateExternalSyncStatusAsync(int policyId, bool synced, DateTime? lastSyncDate = null);
        Task<int> GetExpiringPoliciesCountAsync(int tenantId, int daysAhead);
    }
}