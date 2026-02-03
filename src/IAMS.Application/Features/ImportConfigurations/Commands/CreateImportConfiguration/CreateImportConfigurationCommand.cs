using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.ImportConfigurations.Commands.CreateImportConfiguration
{
    public class CreateImportConfigurationCommand : IRequest<Result<ImportConfigurationDto>>
    {
        public CreateImportConfigurationDto ConfigurationDto { get; set; }

        public CreateImportConfigurationCommand(CreateImportConfigurationDto configurationDto)
        {
            ConfigurationDto = configurationDto;
        }
    }
}
