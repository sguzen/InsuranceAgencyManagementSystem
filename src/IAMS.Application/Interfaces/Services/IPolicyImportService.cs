using IAMS.Shared.QueryParams;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Policy;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Maps imported policy data to domain entities
    /// </summary>
    public interface IPolicyImportService
    {
        /// <summary>
        /// Imports a single policy from DTO
        /// </summary>
        Task<Policy> ImportPolicyAsync(ImportPolicyDto policyDto, string userId, CancellationToken cancellationToken = default);
    }
}
