using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IPolicyRepository : IRepository<Policy>
    {
        // Basic CRUD operations
        Task<Policy?> GetPolicyByIdWithDetailsAsync(int id);
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber);
        Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<bool> PolicyNumberExistsAsync(string policyNumber, int? excludePolicyId = null);

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

        // Count queries
        Task<int> GetPolicyCountAsync();
        Task<int> GetActivePolicyCountAsync();
        Task<int> GetExpiringPolicyCountAsync(int daysAhead);
        Task<int> GetExpiredPolicyCountAsync();
        Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync();

        // Revenue queries
        Task<decimal> GetMonthlyRevenueAsync(DateTime? month = null);
        Task<Dictionary<string, decimal>> GetMonthlyRevenueByCurrencyAsync(DateTime? month = null);
        Task<decimal> GetYearlyRevenueAsync(int? year = null);
        Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int months = 12);
        Task<decimal> GetTotalPremiumByCustomerAsync(int customerId);

        // Business rule validation
        Task<bool> HasOverduePaymentsAsync(int policyId);
        Task<bool> CanBeCancelledAsync(int policyId);
        Task<bool> CanBeRenewedAsync(int policyId);

        // Advanced queries
        Task<List<Policy>> GetPoliciesExpiringInDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Policy>> SearchPoliciesAsync(string searchTerm);
        Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId);
        Task<List<Policy>> GetPoliciesByPolicyTypeAsync(int policyTypeId);

        // Reporting queries
        Task<List<Policy>> GetPoliciesForReportAsync(DateTime? startDate, DateTime? endDate, PolicyStatus? status);
        Task<Dictionary<string, object>> GetPolicyAnalyticsAsync();

        // Integration support
        Task<List<Policy>> GetPoliciesModifiedAfterAsync(DateTime modifiedDate);
        Task<Policy?> GetPolicyByExternalIdAsync(string externalId, int insuranceCompanyId);
        Task UpdateExternalSyncStatusAsync(int policyId, bool synced, DateTime? lastSyncDate = null);
        Task<int> GetExpiringPoliciesCountAsync(int daysAhead);
        Task<List<Policy>> GetByVehicleIdAsync(int id);
    }
}