using AutoMapper;
using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfigurations
{
    public class GetImportConfigurationsQueryHandler
        : IRequestHandler<GetImportConfigurationsQuery, Result<List<ImportConfigurationDto>>>
    {
        private readonly IImportConfigurationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetImportConfigurationsQueryHandler> _logger;

        public GetImportConfigurationsQueryHandler(
            IImportConfigurationRepository repository,
            IMapper mapper,
            ILogger<GetImportConfigurationsQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<ImportConfigurationDto>>> Handle(
            GetImportConfigurationsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var configurations = await _repository.GetActiveConfigurationsAsync();
                var dtos = _mapper.Map<List<ImportConfigurationDto>>(configurations);
                return Result<List<ImportConfigurationDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving import configurations");
                return Result<List<ImportConfigurationDto>>.InternalError(
                    "Import konfigürasyonları getirilirken bir hata oluştu");
            }
        }
    }
}
