using FluentValidation;

namespace IAMS.Application.Features.Parametric.Commands.SyncCountryData
{
    /// <summary>
    /// Validator for SyncCountryDataCommand
    /// </summary>
    public class SyncCountryDataCommandValidator : AbstractValidator<SyncCountryDataCommand>
    {
        public SyncCountryDataCommandValidator()
        {
            // No specific validation rules needed for boolean flags
            // Command is always valid
        }
    }
}
