using FluentValidation;

namespace IAMS.Application.Features.Vehicles.Commands.SyncVehicleData
{
    /// <summary>
    /// Validator for SyncVehicleDataCommand
    /// </summary>
    public class SyncVehicleDataCommandValidator : AbstractValidator<SyncVehicleDataCommand>
    {
        public SyncVehicleDataCommandValidator()
        {
            // No required fields - all parameters have default values
            // UpdateExisting and DeactivateMissing are boolean flags with defaults
        }
    }
}
