using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfigurations
{
    public class GetImportConfigurationsQuery : IRequest<Result<List<ImportConfigurationDto>>>
    {
        public GetImportConfigurationsQuery() { }
    }
}
