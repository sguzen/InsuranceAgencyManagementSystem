using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.ImportConfigurations.Commands.UpdateImportConfiguration
{
    public record UpdateImportConfigurationCommand(int Id, UpdateImportConfigurationDto ConfigurationDto)
        : IRequest<Result<ImportConfigurationDto>>;
}
