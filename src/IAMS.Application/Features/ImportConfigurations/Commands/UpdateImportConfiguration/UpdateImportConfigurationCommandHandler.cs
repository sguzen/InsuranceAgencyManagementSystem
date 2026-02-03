using AutoMapper;
using IAMS.Application.Interfaces;
using IAMS.Shared.DTOs.ImportConfiguration;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.ImportConfigurations.Commands.UpdateImportConfiguration
{
    public class UpdateImportConfigurationCommandHandler
        : IRequestHandler<UpdateImportConfigurationCommand, Result<ImportConfigurationDto>>
    {
        private readonly IImportConfigurationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateImportConfigurationCommandHandler> _logger;

        public UpdateImportConfigurationCommandHandler(
            IImportConfigurationRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            ILogger<UpdateImportConfigurationCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<ImportConfigurationDto>> Handle(
            UpdateImportConfigurationCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating import configuration with ID: {Id}", request.Id);

                var configuration = await _repository.GetByIdAsync(request.Id);
                if (configuration == null)
                {
                    return Result<ImportConfigurationDto>.NotFound("Import konfigürasyonu bulunamadı");
                }

                var dto = request.ConfigurationDto;

                // Check name uniqueness if changed
                if (configuration.Name != dto.Name)
                {
                    var existing = await _repository.GetByNameAsync(dto.Name);
                    if (existing != null && existing.Id != request.Id)
                    {
                        return Result<ImportConfigurationDto>.Conflict(
                            "Bu isimde bir import konfigürasyonu zaten mevcut");
                    }
                }

                // Validate insurance company if changed
                if (configuration.InsuranceCompanyId != dto.InsuranceCompanyId)
                {
                    var insuranceCompany = await _unitOfWork.InsuranceCompanies.GetByIdAsync(dto.InsuranceCompanyId);
                    if (insuranceCompany == null)
                    {
                        return Result<ImportConfigurationDto>.Failure(
                            $"Sigorta şirketi bulunamadı: ID {dto.InsuranceCompanyId}", (List<string>?)null);
                    }
                }

                // Update properties
                configuration.Name = dto.Name;
                configuration.Description = dto.Description;
                configuration.InsuranceCompanyId = dto.InsuranceCompanyId;
                configuration.SourceType = dto.SourceType;
                configuration.ApiBaseUrl = dto.ApiBaseUrl;
                configuration.ApiKey = dto.ApiKey;
                configuration.ApiUsername = dto.ApiUsername;
                configuration.PoliciesEndpoint = dto.PoliciesEndpoint;
                configuration.CustomHeaders = dto.CustomHeaders;
                configuration.AdditionalSettings = dto.AdditionalSettings;
                configuration.IsActive = dto.IsActive;
                configuration.EnableAutoSync = dto.EnableAutoSync;
                configuration.SyncIntervalMinutes = dto.SyncIntervalMinutes;

                // Only update password if provided (non-null means user wants to change it)
                if (dto.ApiPassword != null)
                {
                    configuration.ApiPassword = dto.ApiPassword;
                }

                configuration.UpdateAuditInfo(_currentUserService.UserName ?? "System");

                _repository.Update(configuration);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var resultDto = _mapper.Map<ImportConfigurationDto>(configuration);

                _logger.LogInformation("Import configuration updated: {Id}", configuration.Id);

                return Result<ImportConfigurationDto>.Success(resultDto,
                    "Import konfigürasyonu başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating import configuration with ID: {Id}", request.Id);
                return Result<ImportConfigurationDto>.InternalError(
                    "Import konfigürasyonu güncellenirken bir hata oluştu");
            }
        }
    }
}
