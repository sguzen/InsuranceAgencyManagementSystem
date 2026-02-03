using AutoMapper;
using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfigurationsByInsuranceCompany
{
    public class GetImportConfigurationsByInsuranceCompanyQueryHandler
        : IRequestHandler<GetImportConfigurationsByInsuranceCompanyQuery, Result<List<ImportConfigurationDto>>>
    {
        private readonly IImportConfigurationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetImportConfigurationsByInsuranceCompanyQueryHandler> _logger;

        public GetImportConfigurationsByInsuranceCompanyQueryHandler(
            IImportConfigurationRepository repository,
            IMapper mapper,
            ILogger<GetImportConfigurationsByInsuranceCompanyQueryHandler> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<ImportConfigurationDto>>> Handle(
            GetImportConfigurationsByInsuranceCompanyQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var configurations = await _repository.GetByInsuranceCompanyIdAsync(request.InsuranceCompanyId);
                var dtos = _mapper.Map<List<ImportConfigurationDto>>(configurations);
                return Result<List<ImportConfigurationDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving import configurations for insurance company: {Id}",
                    request.InsuranceCompanyId);
                return Result<List<ImportConfigurationDto>>.InternalError(
                    "Import konfigürasyonları getirilirken bir hata oluştu");
            }
        }
    }
}
