using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for policy integration and external system synchronization
    /// </summary>
    public interface IPolicyIntegrationService
    {
        /// <summary>
        /// Gets policies modified after a specific date
        /// </summary>
        Task<List<Policy>> GetPoliciesModifiedAfterAsync(DateTime modifiedDate);

        /// <summary>
        /// Gets a policy by external system ID
        /// </summary>
        Task<Policy?> GetPolicyByExternalIdAsync(string externalId, int insuranceCompanyId);

        /// <summary>
        /// Updates the external sync status of a policy
        /// </summary>
        Task UpdateExternalSyncStatusAsync(int policyId, bool synced, DateTime? lastSyncDate = null);
    }
}
