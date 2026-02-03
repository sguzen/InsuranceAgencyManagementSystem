using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfiguration
{
    public class GetImportConfigurationQuery : IRequest<Result<ImportConfigurationDto>>
    {
        public int Id { get; set; }

        public GetImportConfigurationQuery(int id)
        {
            Id = id;
        }
    }
}
