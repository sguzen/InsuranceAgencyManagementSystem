using IAMS.Application.DTOs.Policy;
using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for importing policies from external API sources
    /// </summary>
    public interface IExternalPolicyImportService
    {
        /// <summary>
        /// Fetch policies from external API using configuration
        /// </summary>
        Task<List<ImportPolicyDto>> FetchPoliciesAsync(
            ImportConfiguration configuration,
            DateTime? startDate = null,
            DateTime? endDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Test API connection with given configuration
        /// </summary>
        Task<bool> TestConnectionAsync(
            ImportConfiguration configuration,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get available insurance companies from external API
        /// </summary>
        Task<List<string>> GetAvailableInsuranceCompaniesAsync(
            ImportConfiguration configuration,
            CancellationToken cancellationToken = default);
    }
}
