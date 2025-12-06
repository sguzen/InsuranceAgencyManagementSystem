using IAMS.Application.DTOs.Parametric;

namespace IAMS.Application.Interfaces
{
    /// <summary>
    /// Service for fetching country data from external DAS API
    /// </summary>
    public interface ICountryDataService
    {
        /// <summary>
        /// Fetches all countries from external DAS API
        /// </summary>
        /// <returns>List of country data from external source</returns>
        Task<List<ExternalCountryDataDto>> FetchCountryDataAsync();

        /// <summary>
        /// Tests connection to the external country data API
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();
    }
}
