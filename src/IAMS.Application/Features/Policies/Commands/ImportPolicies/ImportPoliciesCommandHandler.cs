using IAMS.Application.DTOs.Policy;
using IAMS.Application.Models;
using IAMS.Application.Services.Policies;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.ImportPolicies
{
    public class ImportPoliciesCommandHandler : IRequestHandler<ImportPoliciesCommand, Result<PolicyImportResultDto>>
    {
        private readonly IPolicyImportService _policyImportService;
        private readonly ILogger<ImportPoliciesCommandHandler> _logger;

        public ImportPoliciesCommandHandler(
            IPolicyImportService policyImportService,
            ILogger<ImportPoliciesCommandHandler> logger)
        {
            _policyImportService = policyImportService;
            _logger = logger;
        }

        public async Task<Result<PolicyImportResultDto>> Handle(ImportPoliciesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate file
                if (request.File == null || request.File.Length == 0)
                {
                    return Result<PolicyImportResultDto>.Failure("No file was uploaded");
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (fileExtension != ".xlsx" && fileExtension != ".xls")
                {
                    return Result<PolicyImportResultDto>.Failure("Invalid file format. Only Excel files (.xlsx, .xls) are supported");
                }

                // Validate file size (max 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (request.File.Length > maxFileSize)
                {
                    return Result<PolicyImportResultDto>.Failure("File size exceeds the maximum allowed size of 10MB");
                }

                _logger.LogInformation("Starting policy import from file: {FileName}, Size: {FileSize} bytes",
                    request.File.FileName, request.File.Length);

                // Import policies from stream
                PolicyImportResultDto result;
                using (var stream = request.File.OpenReadStream())
                {
                    result = await _policyImportService.ImportFromStreamAsync(stream, request.UserId);
                }

                _logger.LogInformation("Policy import completed. Total: {Total}, Success: {Success}, Failure: {Failure}",
                    result.TotalRows, result.SuccessCount, result.FailureCount);

                return Result<PolicyImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing policies from file: {FileName}", request.File?.FileName);
                return Result<PolicyImportResultDto>.Failure($"Failed to import policies: {ex.Message}");
            }
        }
    }
}
