using IAMS.Shared.QueryParams;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for querying and searching policies
    /// </summary>
    public class PolicyQueryService : IPolicyQueryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PolicyQueryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Delegate to repository - the existing PolicyRepository still has these methods
        // This provides a cleaner service-based interface
        private dynamic PolicyRepo => (dynamic)_unitOfWork.Policies;

        public Task<PagedResult<Policy>> GetPoliciesAsync(PolicyQueryParams queryParams)
            => PolicyRepo.GetPoliciesAsync(queryParams);

        public Task<PagedResult<Policy>> GetPoliciesPagedAsync(int pageNumber, int pageSize, string? searchTerm = null)
            => PolicyRepo.GetPoliciesPagedAsync(pageNumber, pageSize, searchTerm);

        public Task<PagedResult<Policy>> GetPoliciesByStatusPagedAsync(PolicyStatus status, int pageNumber, int pageSize)
            => PolicyRepo.GetPoliciesByStatusPagedAsync(status, pageNumber, pageSize);

        public Task<List<Policy>> GetActivePoliciesAsync()
            => PolicyRepo.GetActivePoliciesAsync();

        public Task<List<Policy>> GetPoliciesByStatusAsync(PolicyStatus status)
            => PolicyRepo.GetPoliciesByStatusAsync(status);

        public Task<List<Policy>> GetExpiringPoliciesAsync(int daysAhead)
            => PolicyRepo.GetExpiringPoliciesAsync(daysAhead);

        public Task<List<Policy>> GetExpiredPoliciesAsync()
            => PolicyRepo.GetExpiredPoliciesAsync();

        public Task<List<Policy>> GetRecentPoliciesAsync(int count)
            => PolicyRepo.GetRecentPoliciesAsync(count);

        public Task<List<Policy>> GetTopPoliciesByPremiumAsync(int count)
            => PolicyRepo.GetTopPoliciesByPremiumAsync(count);

        public Task<List<Policy>> GetPoliciesExpiringInDateRangeAsync(DateTime startDate, DateTime endDate)
            => PolicyRepo.GetPoliciesExpiringInDateRangeAsync(startDate, endDate);

        public Task<List<Policy>> SearchPoliciesAsync(string searchTerm)
            => PolicyRepo.SearchPoliciesAsync(searchTerm);

        public Task<List<Policy>> GetPoliciesByInsuranceCompanyAsync(int insuranceCompanyId)
            => PolicyRepo.GetPoliciesByInsuranceCompanyAsync(insuranceCompanyId);

        public Task<List<Policy>> GetPoliciesByPolicyTypeAsync(int policyTypeId)
            => PolicyRepo.GetPoliciesByPolicyTypeAsync(policyTypeId);

        public Task<List<Policy>> GetByVehicleIdAsync(int vehicleId)
            => PolicyRepo.GetByVehicleIdAsync(vehicleId);

        public Task<List<Policy>> GetEndorsementsByOriginalPolicyIdAsync(int originalPolicyId)
            => PolicyRepo.GetEndorsementsByOriginalPolicyIdAsync(originalPolicyId);

        public Task<int> GetEndorsementCountAsync(int originalPolicyId)
            => PolicyRepo.GetEndorsementCountAsync(originalPolicyId);

        public Task<bool> HasOverduePaymentsAsync(int policyId)
            => PolicyRepo.HasOverduePaymentsAsync(policyId);

        public Task<bool> CanBeCancelledAsync(int policyId)
            => PolicyRepo.CanBeCancelledAsync(policyId);

        public Task<bool> CanBeRenewedAsync(int policyId)
            => PolicyRepo.CanBeRenewedAsync(policyId);
    }
}
