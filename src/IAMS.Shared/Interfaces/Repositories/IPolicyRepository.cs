using IAMS.Domain.Entities;

namespace IAMS.Shared.Interfaces.Repositories
{
    /// <summary>
    /// Repository for core Policy CRUD operations only
    /// For specialized operations use:
    /// - IPolicyQueryService (searches, filters, paging)
    /// - IPolicyAnalyticsService (counts, revenue, statistics)
    /// - IPolicyReportingService (reporting queries)
    /// - IPolicyIntegrationService (external system sync)
    /// </summary>
    public interface IPolicyRepository : IRepository<Policy>
    {
        // Core CRUD operations only
        Task<Policy?> GetPolicyByIdWithDetailsAsync(int id);
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber); // Gets main policy (InnerCode = "000")
        Task<Policy?> GetByPolicyNumberAndInnerCodeAsync(string policyNumber, string innerCode);
        Task<List<Policy>> GetAllByPolicyNumberAsync(string policyNumber); // Gets all policies + endorsements
        Task<List<Policy>> GetPoliciesByCustomerIdAsync(int customerId);
        Task<bool> PolicyNumberExistsAsync(string policyNumber, int? excludePolicyId = null);
        Task<bool> PolicyNumberAndInnerCodeExistsAsync(string policyNumber, string innerCode, int? excludePolicyId = null);
    }
}
