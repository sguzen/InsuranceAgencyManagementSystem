using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for policy reporting operations
    /// </summary>
    public class PolicyReportingService : IPolicyReportingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PolicyReportingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Delegate to repository
        private dynamic PolicyRepo => (dynamic)_unitOfWork.Policies;

        public Task<List<Policy>> GetPoliciesForReportAsync(
            DateTime? startDate,
            DateTime? endDate,
            PolicyStatus? status)
            => PolicyRepo.GetPoliciesForReportAsync(startDate, endDate, status);
    }
}
