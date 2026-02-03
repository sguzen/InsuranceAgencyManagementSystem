using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.ImportConfigurations.Commands.DeleteImportConfiguration
{
    public record DeleteImportConfigurationCommand(int Id) : IRequest<Result>;
}
