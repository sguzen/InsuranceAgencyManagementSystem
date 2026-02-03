using AutoMapper;
using IAMS.Application.Interfaces;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.ImportConfigurations.Commands.CreateImportConfiguration
{
    public class CreateImportConfigurationCommandHandler
        : IRequestHandler<CreateImportConfigurationCommand, Result<ImportConfigurationDto>>
    {
        private readonly IImportConfigurationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateImportConfigurationCommandHandler> _logger;

        public CreateImportConfigurationCommandHandler(
            IImportConfigurationRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<CreateImportConfigurationCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ImportConfigurationDto>> Handle(
            CreateImportConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                var dto = request.ConfigurationDto;
                _logger.LogInformation("Creating import configuration: {Name} for InsuranceCompanyId={CompanyId}",
                    dto.Name, dto.InsuranceCompanyId);

                // Check if name already exists
                var existing = await _repository.GetByNameAsync(dto.Name);
                if (existing != null)
                {
                    return Result<ImportConfigurationDto>.Conflict(
                        "Bu isimde bir import konfigürasyonu zaten mevcut");
                }

                // Validate insurance company exists
                var insuranceCompany = await _unitOfWork.InsuranceCompanies.GetByIdAsync(dto.InsuranceCompanyId);
                if (insuranceCompany == null)
                {
                    return Result<ImportConfigurationDto>.Failure(
                        $"Sigorta şirketi bulunamadı: ID {dto.InsuranceCompanyId}", (List<string>?)null);
                }

                var configuration = _mapper.Map<ImportConfiguration>(dto);
                configuration.CreatedBy = _currentUserService.UserName ?? "System";
                configuration.CreatedOn = DateTime.UtcNow;

                await _repository.AddAsync(configuration);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var resultDto = _mapper.Map<ImportConfigurationDto>(configuration);
                resultDto.InsuranceCompanyName = insuranceCompany.Name;

                _logger.LogInformation("Import configuration created with ID: {Id}", configuration.Id);

                return Result<ImportConfigurationDto>.Success(resultDto,
                    "Import konfigürasyonu başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating import configuration: {Name}",
                    request.ConfigurationDto.Name);
                return Result<ImportConfigurationDto>.InternalError(
                    "Import konfigürasyonu oluşturulurken bir hata oluştu");
            }
        }
    }
}
