using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.ParsePolicyImport
{
    public class ParsePolicyImportQueryHandler : IRequestHandler<ParsePolicyImportQuery, Result<List<PolicyImportPreviewDto>>>
    {
        private readonly IExcelFileValidator _fileValidator;
        private readonly IExcelPolicyParser _policyParser;
        private readonly ILogger<ParsePolicyImportQueryHandler> _logger;

        public ParsePolicyImportQueryHandler(
            IExcelFileValidator fileValidator,
            IExcelPolicyParser policyParser,
            ILogger<ParsePolicyImportQueryHandler> logger)
        {
            _fileValidator = fileValidator;
            _policyParser = policyParser;
            _logger = logger;
        }

        public async Task<Result<List<PolicyImportPreviewDto>>> Handle(ParsePolicyImportQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate file
                var validationResult = _fileValidator.ValidateFile(request.File);
                if (!validationResult.IsSuccess)
                {
                    return Result<List<PolicyImportPreviewDto>>.Failure(validationResult.Message, (List<string>?)null);
                }

                // Validate insurance company ID
                if (request.InsuranceCompanyId <= 0)
                {
                    return Result<List<PolicyImportPreviewDto>>.Failure("Insurance company must be selected", (List<string>?)null);
                }

                _logger.LogInformation("Parsing policy import file for preview: {FileName}, Size: {FileSize} bytes",
                    request.File.FileName, request.File.Length);

                // Parse Excel file
                List<ImportPolicyDto> importedPolicies;
                using (var stream = request.File.OpenReadStream())
                {
                    importedPolicies = await _policyParser.ParseFromStreamAsync(stream, cancellationToken);
                }

                _logger.LogInformation("Parsed {Count} policies from file", importedPolicies.Count);

                // Convert to preview DTOs
                // No customer lookup needed - Sigortalı will be auto-created from Excel data during import
                // Operator only needs to specify Policy Owner
                var previewDtos = importedPolicies.Select(dto => new PolicyImportPreviewDto
                {
                    RowNumber = dto.RowNumber,
                    PolicyNumber = dto.PolicyNumber,
                    InnerCode = dto.InnerCode,
                    StateType = dto.StateType,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    PolicyTypeCode = dto.PolicyTypeCode,
                    PolicyTypeName = dto.PolicyTypeName,
                    PremiumAmount = dto.PremiumAmount,
                    CommissionRate = dto.CommissionRate,
                    CommissionAmount = dto.CommissionAmount,
                    CurrencyCode = dto.CurrencyCode,
                    PlateNumber = dto.PlateNumber,
                    VehicleBrand = dto.VehicleBrand,
                    VehicleModel = dto.VehicleModel,
                    VehicleYear = dto.VehicleYear,
                    InsuredCustomerName = dto.CustomerName,
                    InsuredCustomerIdentifier = dto.CustomerIdentifier,
                    PolicyOwnerSameAsInsured = true, // Default: policy owner = sigortalı
                    OriginalImportData = dto
                }).ToList();

                return Result<List<PolicyImportPreviewDto>>.Success(
                    previewDtos,
                    $"Successfully parsed {previewDtos.Count} policies. Review and specify policy owners before importing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing policy import file: {FileName}", request.File?.FileName);
                return Result<List<PolicyImportPreviewDto>>.Failure($"Failed to parse file: {ex.Message}", (List<string>?)null);
            }
        }
    }
}
