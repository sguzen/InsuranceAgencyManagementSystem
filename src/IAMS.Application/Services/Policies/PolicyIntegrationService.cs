using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for policy integration and external system synchronization
    /// </summary>
    public class PolicyIntegrationService : IPolicyIntegrationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PolicyIntegrationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Delegate to repository
        private dynamic PolicyRepo => (dynamic)_unitOfWork.Policies;

        public Task<List<Policy>> GetPoliciesModifiedAfterAsync(DateTime modifiedDate)
            => PolicyRepo.GetPoliciesModifiedAfterAsync(modifiedDate);

        public Task<Policy?> GetPolicyByExternalIdAsync(string externalId, int insuranceCompanyId)
            => PolicyRepo.GetPolicyByExternalIdAsync(externalId, insuranceCompanyId);

        public Task UpdateExternalSyncStatusAsync(int policyId, bool synced, DateTime? lastSyncDate = null)
            => PolicyRepo.UpdateExternalSyncStatusAsync(policyId, synced, lastSyncDate);
    }
}
