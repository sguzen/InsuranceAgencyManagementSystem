using IAMS.Shared.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Commands.SyncVehicleData
{
    /// <summary>
    /// Command to synchronize vehicle brands and models from external API
    /// </summary>
    public class SyncVehicleDataCommand : IRequest<Result<VehicleDataSyncResult>>
    {
        /// <summary>
        /// Whether to update existing records or only create new ones
        /// </summary>
        public bool UpdateExisting { get; set; } = true;

        /// <summary>
        /// Whether to deactivate brands/models not found in external data
        /// </summary>
        public bool DeactivateMissing { get; set; } = false;
    }
}
