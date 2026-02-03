using AutoMapper;
using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.ImportConfigurations.Queries.GetImportConfiguration
{
    public class GetImportConfigurationQueryHandler
        : IRequestHandler<GetImportConfigurationQuery, Result<ImportConfigurationDto>>
    {
        private readonly IImportConfigurationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetImportConfigurationQueryHandler> _logger;

        public GetImportConfigurationQueryHandler(
            IImportConfigurationRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetImportConfigurationQueryHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ImportConfigurationDto>> Handle(
            GetImportConfigurationQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var configuration = await _repository.GetByIdAsync(request.Id);
                if (configuration == null)
                {
                    return Result<ImportConfigurationDto>.NotFound("Import konfigürasyonu bulunamadı");
                }

                var dto = _mapper.Map<ImportConfigurationDto>(configuration);

                // Load insurance company name (base GetByIdAsync doesn't include navigation)
                var company = await _unitOfWork.InsuranceCompanies.GetByIdAsync(configuration.InsuranceCompanyId);
                dto.InsuranceCompanyName = company?.Name;

                return Result<ImportConfigurationDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving import configuration with ID: {Id}", request.Id);
                return Result<ImportConfigurationDto>.InternalError(
                    "Import konfigürasyonu getirilirken bir hata oluştu");
            }
        }
    }
}
