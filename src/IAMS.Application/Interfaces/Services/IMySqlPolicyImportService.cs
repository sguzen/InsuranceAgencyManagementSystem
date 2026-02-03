using IAMS.Shared.DTOs.Policy;

namespace IAMS.Application.Interfaces.Services
{
    /// <summary>
    /// Service for importing policies from an external MySQL database
    /// </summary>
    public interface IMySqlPolicyImportService
    {
        /// <summary>
        /// Fetch policies from the external MySQL database
        /// </summary>
        /// <param name="agencyCode">Agency code to filter by (e.g., "A022")</param>
        /// <param name="startDate">Start date for the query range</param>
        /// <param name="endDate">End date for the query range</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parsed policy DTOs</returns>
        Task<List<ImportPolicyDto>> FetchPoliciesAsync(
            string agencyCode,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Test the MySQL database connection
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
    }
}
