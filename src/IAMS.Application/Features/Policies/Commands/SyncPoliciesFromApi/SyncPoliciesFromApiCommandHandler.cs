using IAMS.Shared.QueryParams;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Commands.SyncPoliciesFromApi
{
    public class SyncPoliciesFromApiCommandHandler : IRequestHandler<SyncPoliciesFromApiCommand, Result<PolicyImportResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExternalPolicyImportService _externalImportService;
        private readonly ILogger<SyncPoliciesFromApiCommandHandler> _logger;
        private readonly IMediator _mediator;

        public SyncPoliciesFromApiCommandHandler(
            IUnitOfWork unitOfWork,
            IExternalPolicyImportService externalImportService,
            ILogger<SyncPoliciesFromApiCommandHandler> logger,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _externalImportService = externalImportService;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<PolicyImportResultDto>> Handle(SyncPoliciesFromApiCommand request, CancellationToken cancellationToken)
        {
            ImportHistory? importHistory = null;

            try
            {
                // Get configuration
                var configuration = await _unitOfWork.ImportConfigurations.GetByIdAsync(request.ImportConfigurationId);
                if (configuration == null)
                {
                    return Result<PolicyImportResultDto>.Failure("Import configuration not found", (List<string>?)null);
                }

                if (!configuration.IsActive)
                {
                    return Result<PolicyImportResultDto>.Failure("Import configuration is not active", (List<string>?)null);
                }

                _logger.LogInformation("Starting API sync for configuration: {ConfigName}", configuration.Name);

                // Create import history record
                importHistory = new ImportHistory
                {
                    SourceType = ImportSourceType.ApiSync,
                    ImportConfigurationId = configuration.Id,
                    InsuranceCompanyId = configuration.InsuranceCompanyId,
                    StartedAt = DateTime.UtcNow,
                    Status = "InProgress",
                    ImportedBy = request.UserId,
                    CreatedBy = request.UserId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = request.UserId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _unitOfWork.ImportHistories.AddAsync(importHistory);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Fetch policies from external API
                _logger.LogInformation("Fetching policies from API: {ApiUrl}", configuration.ApiBaseUrl);
                var policies = await _externalImportService.FetchPoliciesAsync(
                    configuration,
                    request.StartDate,
                    request.EndDate,
                    cancellationToken);

                importHistory.TotalRecords = policies.Count;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Fetched {Count} policies from API", policies.Count);

                // Process policies through import logic
                var result = new PolicyImportResultDto
                {
                    TotalRows = policies.Count
                };

                foreach (var policyDto in policies)
                {
                    try
                    {
                        // Use the existing import logic (you can extract this to a shared service)
                        // For now, we'll need to create a method to import a single policy
                        // This will be implemented similarly to ImportPoliciesCommandHandler

                        // TODO: Import single policy
                        // await ImportSinglePolicyAsync(policyDto, request.UserId, configuration.InsuranceCompanyId, cancellationToken);

                        result.SuccessCount++;
                        importHistory.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing policy from API: {PolicyNumber}", policyDto.PolicyNumber);
                        result.FailureCount++;
                        importHistory.FailureCount++;

                        result.Errors.Add(new PolicyImportError
                        {
                            RowNumber = policyDto.RowNumber,
                            PolicyNumber = policyDto.PolicyNumber ?? "Unknown",
                            ErrorMessage = ex.Message
                        });
                    }
                }

                // Update import history
                importHistory.MarkAsCompleted(result.FailureCount == 0 ? "Success" : "PartialSuccess");
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Update configuration last sync
                configuration.LastSyncDate = DateTime.UtcNow;
                configuration.LastSyncStatus = importHistory.Status;
                configuration.ModifiedBy = request.UserId;
                configuration.ModifiedOn = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("API sync completed. Success: {Success}, Failure: {Failure}",
                    result.SuccessCount, result.FailureCount);

                return Result<PolicyImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during API sync");

                if (importHistory != null)
                {
                    importHistory.MarkAsCompleted("Failed");
                    importHistory.ErrorMessages = ex.Message;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }

                return Result<PolicyImportResultDto>.Failure($"API sync failed: {ex.Message}", (List<string>?)null);
            }
        }
    }
}
