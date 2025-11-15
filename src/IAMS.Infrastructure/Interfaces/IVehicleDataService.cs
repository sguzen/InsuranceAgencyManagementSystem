using IAMS.Application.DTOs.Vehicle;

namespace IAMS.Infrastructure.Interfaces
{
    /// <summary>
    /// Service for fetching vehicle brand and model data from external API
    /// </summary>
    public interface IVehicleDataService
    {
        /// <summary>
        /// Fetches all vehicle brands and models from external API
        /// </summary>
        /// <returns>List of vehicle data from external source</returns>
        Task<List<ExternalVehicleDataDto>> FetchVehicleDataAsync();

        /// <summary>
        /// Tests connection to the external vehicle data API
        /// </summary>
        /// <returns>True if connection is successful</returns>
        Task<bool> TestConnectionAsync();
    }
}
