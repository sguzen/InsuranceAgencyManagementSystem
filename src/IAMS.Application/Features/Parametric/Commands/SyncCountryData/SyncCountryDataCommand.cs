using IAMS.Shared.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Commands.SyncCountryData
{
    /// <summary>
    /// Command to synchronize countries from external DAS API
    /// </summary>
    public class SyncCountryDataCommand : IRequest<Result<CountryDataSyncResult>>
    {
        /// <summary>
        /// Whether to update existing records or only create new ones
        /// </summary>
        public bool UpdateExisting { get; set; } = true;

        /// <summary>
        /// Whether to deactivate countries not found in external data
        /// </summary>
        public bool DeactivateMissing { get; set; } = false;
    }
}
