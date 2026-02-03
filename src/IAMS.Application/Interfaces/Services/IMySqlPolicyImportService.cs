using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Policy;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for importing policies from an external MySQL database.
    /// Connection details are provided per-call via ImportConfiguration,
    /// allowing each insurance company to have its own MySQL connection and agency code.
    /// </summary>
    public interface IMySqlPolicyImportService
    {
        /// <summary>
        /// Fetch policies from the external MySQL database using the given import configuration
        /// </summary>
        /// <param name="configuration">Import configuration containing MySQL connection details and agency code</param>
        /// <param name="startDate">Start date for the query range</param>
        /// <param name="endDate">End date for the query range</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parsed policy DTOs</returns>
        Task<List<ImportPolicyDto>> FetchPoliciesAsync(
            ImportConfiguration configuration,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Test the MySQL database connection using the given import configuration
        /// </summary>
        /// <param name="configuration">Import configuration containing MySQL connection details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync(
            ImportConfiguration configuration,
            CancellationToken cancellationToken = default);
    }
}
