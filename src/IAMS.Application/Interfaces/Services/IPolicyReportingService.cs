using IAMS.Domain.Entities;
using IAMS.Domain.Enums;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for policy reporting operations
    /// </summary>
    public interface IPolicyReportingService
    {
        /// <summary>
        /// Gets policies for reporting with optional filters
        /// </summary>
        Task<List<Policy>> GetPoliciesForReportAsync(
            DateTime? startDate,
            DateTime? endDate,
            PolicyStatus? status);
    }
}
