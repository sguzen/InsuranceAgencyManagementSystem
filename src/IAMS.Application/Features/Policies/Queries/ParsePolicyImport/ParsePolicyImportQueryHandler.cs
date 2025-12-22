using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.ParsePolicyImport
{
    public class ParsePolicyImportQueryHandler : IRequestHandler<ParsePolicyImportQuery, Result<List<PolicyImportPreviewDto>>>
    {
        private readonly IExcelFileValidator _fileValidator;
        private readonly IExcelPolicyParser _policyParser;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ParsePolicyImportQueryHandler> _logger;

        public ParsePolicyImportQueryHandler(
            IExcelFileValidator fileValidator,
            IExcelPolicyParser policyParser,
            IUnitOfWork unitOfWork,
            ILogger<ParsePolicyImportQueryHandler> logger)
        {
            _fileValidator = fileValidator;
            _policyParser = policyParser;
            _unitOfWork = unitOfWork;
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

                // Convert to preview DTOs with customer matching
                var previewDtos = new List<PolicyImportPreviewDto>();
                foreach (var importDto in importedPolicies)
                {
                    var previewDto = await CreatePreviewDtoAsync(importDto, cancellationToken);
                    previewDtos.Add(previewDto);
                }

                return Result<List<PolicyImportPreviewDto>>.Success(
                    previewDtos,
                    $"Successfully parsed {previewDtos.Count} policies. Review and map customers before importing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing policy import file: {FileName}", request.File?.FileName);
                return Result<List<PolicyImportPreviewDto>>.Failure($"Failed to parse file: {ex.Message}", (List<string>?)null);
            }
        }

        private async Task<PolicyImportPreviewDto> CreatePreviewDtoAsync(ImportPolicyDto importDto, CancellationToken cancellationToken)
        {
            var previewDto = new PolicyImportPreviewDto
            {
                RowNumber = importDto.RowNumber,
                PolicyNumber = importDto.PolicyNumber,
                InnerCode = importDto.InnerCode,
                StateType = importDto.StateType,
                StartDate = importDto.StartDate,
                EndDate = importDto.EndDate,
                PolicyTypeCode = importDto.PolicyTypeCode,
                PolicyTypeName = importDto.PolicyTypeName,
                PremiumAmount = importDto.PremiumAmount,
                CommissionRate = importDto.CommissionRate,
                CommissionAmount = importDto.CommissionAmount,
                CurrencyCode = importDto.CurrencyCode,
                PlateNumber = importDto.PlateNumber,
                VehicleBrand = importDto.VehicleBrand,
                VehicleModel = importDto.VehicleModel,
                VehicleYear = importDto.VehicleYear,
                InsuredCustomerName = importDto.CustomerName,
                InsuredCustomerIdentifier = importDto.CustomerIdentifier,
                OriginalImportData = importDto
            };

            // Search for suggested customer matches
            if (!string.IsNullOrEmpty(importDto.CustomerIdentifier) || !string.IsNullOrEmpty(importDto.CustomerName))
            {
                previewDto.SuggestedInsuredMatches = await FindCustomerMatchesAsync(
                    importDto.CustomerName,
                    importDto.CustomerIdentifier,
                    cancellationToken);

                // Auto-select if exact match found by identifier
                var exactMatch = previewDto.SuggestedInsuredMatches
                    .FirstOrDefault(m => m.MatchScore >= 100);
                if (exactMatch != null)
                {
                    previewDto.InsuredCustomerId = exactMatch.CustomerId;
                    previewDto.PolicyOwnerSameAsInsured = true; // Default to same
                }
            }

            return previewDto;
        }

        private async Task<List<CustomerMatchDto>> FindCustomerMatchesAsync(
            string? customerName,
            string? customerIdentifier,
            CancellationToken cancellationToken)
        {
            var matches = new List<CustomerMatchDto>();

            // Search by identifier (exact match = 100 score)
            if (!string.IsNullOrEmpty(customerIdentifier))
            {
                var customerByIdentifier = await _unitOfWork.Customers.GetByIdentificationNoAsync(customerIdentifier);
                if (customerByIdentifier != null)
                {
                    matches.Add(new CustomerMatchDto
                    {
                        CustomerId = customerByIdentifier.Id,
                        CustomerCode = customerByIdentifier.CustomerCode,
                        FullName = customerByIdentifier.FullName,
                        IdentificationNumber = customerByIdentifier.IdentificationNumber,
                        Phone = customerByIdentifier.Phone,
                        Email = customerByIdentifier.Email,
                        MatchScore = 100 // Exact match by identifier
                    });
                    return matches; // Return only exact match
                }
            }

            // Search by name (fuzzy match)
            if (!string.IsNullOrEmpty(customerName))
            {
                // Get customers with similar names (top 5)
                var allCustomers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);
                var nameMatches = allCustomers
                    .Where(c => !string.IsNullOrEmpty(c.FirstName) || !string.IsNullOrEmpty(c.LastName))
                    .Select(c => new
                    {
                        Customer = c,
                        Score = CalculateNameMatchScore(customerName, c.FullName)
                    })
                    .Where(x => x.Score > 50) // Only include reasonable matches
                    .OrderByDescending(x => x.Score)
                    .Take(5)
                    .ToList();

                foreach (var match in nameMatches)
                {
                    matches.Add(new CustomerMatchDto
                    {
                        CustomerId = match.Customer.Id,
                        CustomerCode = match.Customer.CustomerCode,
                        FullName = match.Customer.FullName,
                        IdentificationNumber = match.Customer.IdentificationNumber,
                        Phone = match.Customer.Phone,
                        Email = match.Customer.Email,
                        MatchScore = match.Score
                    });
                }
            }

            return matches;
        }

        private int CalculateNameMatchScore(string name1, string name2)
        {
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                return 0;

            var normalized1 = name1.ToLowerInvariant().Trim();
            var normalized2 = name2.ToLowerInvariant().Trim();

            // Exact match
            if (normalized1 == normalized2)
                return 95; // Not 100 because identifier match is perfect

            // Contains match
            if (normalized1.Contains(normalized2) || normalized2.Contains(normalized1))
                return 75;

            // Word overlap
            var words1 = normalized1.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var words2 = normalized2.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var commonWords = words1.Intersect(words2).Count();
            var totalWords = Math.Max(words1.Length, words2.Length);

            if (commonWords > 0)
                return (int)((double)commonWords / totalWords * 70);

            return 0;
        }
    }
}
