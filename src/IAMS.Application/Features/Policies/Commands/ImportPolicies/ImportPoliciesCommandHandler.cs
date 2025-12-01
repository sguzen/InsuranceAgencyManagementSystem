using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IAMS.Application.Features.Policies.Commands.ImportPolicies
{
    public class ImportPoliciesCommandHandler : IRequestHandler<ImportPoliciesCommand, Result<PolicyImportResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExcelFileValidator _fileValidator;
        private readonly IExcelPolicyParser _policyParser;
        private readonly IPolicyQueryService _policyQueryService;
        private readonly ILogger<ImportPoliciesCommandHandler> _logger;

        public ImportPoliciesCommandHandler(
            IUnitOfWork unitOfWork,
            IExcelFileValidator fileValidator,
            IExcelPolicyParser policyParser,
            IPolicyQueryService policyQueryService,
            ILogger<ImportPoliciesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _fileValidator = fileValidator;
            _policyParser = policyParser;
            _policyQueryService = policyQueryService;
            _logger = logger;
        }

        public async Task<Result<PolicyImportResultDto>> Handle(ImportPoliciesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate file using dedicated validator
                var validationResult = _fileValidator.ValidateFile(request.File);
                if (!validationResult.IsSuccess)
                {
                    return Result<PolicyImportResultDto>.Failure(validationResult.Message, (List<string>?)null);
                }

                _logger.LogInformation("Starting policy import from file: {FileName}, Size: {FileSize} bytes",
                    request.File.FileName, request.File.Length);

                // Import policies from stream
                PolicyImportResultDto result;
                using (var stream = request.File.OpenReadStream())
                {
                    result = await ImportFromStreamAsync(stream, request.UserId, cancellationToken);
                }

                _logger.LogInformation("Policy import completed. Total: {Total}, Success: {Success}, Failure: {Failure}",
                    result.TotalRows, result.SuccessCount, result.FailureCount);

                return Result<PolicyImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing policies from file: {FileName}", request.File?.FileName);
                return Result<PolicyImportResultDto>.Failure($"Failed to import policies: {ex.Message}", (List<string>?)null);
            }
        }

        private async Task<PolicyImportResultDto> ImportFromStreamAsync(Stream stream, string userId, CancellationToken cancellationToken)
        {
            var result = new PolicyImportResultDto();

            try
            {
                // Parse Excel file using dedicated parser
                var policies = await _policyParser.ParseFromStreamAsync(stream, cancellationToken);
                result.TotalRows = policies.Count;

                // Import each policy
                foreach (var policyDto in policies)
                {
                    try
                    {
                        var policy = await ImportSinglePolicyAsync(policyDto, userId, cancellationToken);
                        result.SuccessCount++;
                        result.ImportedPolicyIds.Add(policy.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error importing policy at row {RowNumber}: {PolicyNumber}",
                            policyDto.RowNumber, policyDto.PolicyNumber);

                        result.FailureCount++;
                        result.Errors.Add(new PolicyImportError
                        {
                            RowNumber = policyDto.RowNumber,
                            PolicyNumber = policyDto.PolicyNumber ?? "Unknown",
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Excel file");
                throw new InvalidOperationException($"Failed to read Excel file: {ex.Message}", ex);
            }

            return result;
        }

        // The remaining import logic stays in the handler for now

        private async Task<Policy> ImportSinglePolicyAsync(ImportPolicyDto dto, string userId, CancellationToken cancellationToken)
        {
            // Lookup or create related entities
            var customer = await GetOrCreateCustomerAsync(dto);
            var insuranceCompany = await GetInsuranceCompanyAsync(dto);
            var policyType = await GetPolicyTypeAsync(dto);
            var marketer = await GetOrCreateMarketerAsync(dto, userId);
            var currency = await GetCurrencyAsync(dto.CurrencyCode ?? "TRY");
            var vehicle = await GetOrCreateVehicleAsync(dto, customer.Id, userId);

            // Check if this is an endorsement
            Policy? originalPolicy = null;
            string? endorsementNumber = null;

            if (dto.IsEndorsement && !string.IsNullOrEmpty(dto.PolicyNumber))
            {
                // Find the original policy
                originalPolicy = await _unitOfWork.Policies.GetByPolicyNumberAsync(dto.PolicyNumber);
                if (originalPolicy == null)
                {
                    throw new InvalidOperationException(
                        $"Original policy not found for endorsement: {dto.PolicyNumber}");
                }

                // Generate endorsement number
                endorsementNumber = await GenerateEndorsementNumberAsync(originalPolicy.Id);
            }

            // Create policy entity
            var policy = new Policy
            {
                PolicyNumber = dto.PolicyNumber ?? GeneratePolicyNumber(),
                CustomerId = customer.Id,
                InsuranceCompanyId = insuranceCompany.Id,
                PolicyTypeId = policyType.Id,
                VehicleId = vehicle?.Id,
                StartDate = dto.StartDate ?? DateTime.Today,
                EndDate = dto.EndDate ?? DateTime.Today.AddYears(1),
                PremiumAmount = dto.PremiumAmount,
                CommissionRate = dto.CommissionRate ?? 0,
                CommissionAmount = dto.CommissionAmount ?? (dto.PremiumAmount * (dto.CommissionRate ?? 0) / 100),
                Status = dto.Status,
                CurrencyId = currency.Id,
                Notes = dto.Notes,

                // Endorsement fields
                IsEndorsement = dto.IsEndorsement,
                EndorsementNumber = endorsementNumber ?? dto.EndorsementNumber,
                OriginalPolicyId = originalPolicy?.Id,
                BranchCode = dto.BranchCode,

                // Driver fields
                DriverAge = dto.DriverAge,
                DriverType = ParseDriverType(dto.DriverTypeText),

                // Marketer fields
                MarketerId = marketer?.Id,

                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedOn = DateTime.UtcNow
            };

            // Save policy
            await _unitOfWork.Policies.AddAsync(policy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create initial payment if provided
            if (dto.PaidAmount.HasValue && dto.PaidAmount.Value > 0)
            {
                var payment = new PolicyPayment
                {
                    PolicyId = policy.Id,
                    Amount = dto.PaidAmount.Value,
                    PaymentDate = dto.PaymentDate ?? DateTime.Today,
                    PaymentMethod = ParsePaymentMethod(dto.PaymentMethod),
                    Status = PaymentStatus.Completed,
                    CurrencyId = currency.Id,
                    Notes = "Initial payment from import",
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = userId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _unitOfWork.PolicyPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return policy;
        }

        private async Task<string> GenerateEndorsementNumberAsync(int originalPolicyId)
        {
            // Get all endorsements for this policy
            var endorsements = await _policyQueryService.GetEndorsementsByOriginalPolicyIdAsync(originalPolicyId);

            // Find the highest endorsement number
            int maxNumber = -1;
            foreach (var endorsement in endorsements)
            {
                if (!string.IsNullOrEmpty(endorsement.EndorsementNumber))
                {
                    if (int.TryParse(endorsement.EndorsementNumber, out int number))
                    {
                        maxNumber = Math.Max(maxNumber, number);
                    }
                }
            }

            // Generate next number (000, 001, 002, etc.)
            int nextNumber = maxNumber + 1;
            return nextNumber.ToString("000");
        }

        private async Task<Customer> GetOrCreateCustomerAsync(ImportPolicyDto dto)
        {
            // Try to find existing customer by identifier
            if (!string.IsNullOrEmpty(dto.CustomerIdentifier))
            {
                var customer = await _unitOfWork.Customers.GetByIdentificationNoAsync(dto.CustomerIdentifier);
                if (customer != null)
                {
                    return customer;
                }
            }

            // Customer not found - this should not happen in import scenario
            // The user should ensure customers exist before importing policies
            throw new InvalidOperationException(
                $"Customer not found: {dto.CustomerIdentifier}. Please create the customer before importing policies.");
        }

        private async Task<InsuranceCompany> GetInsuranceCompanyAsync(ImportPolicyDto dto)
        {
            InsuranceCompany? company = null;

            if (!string.IsNullOrEmpty(dto.InsuranceCompanyCode))
            {
                company = await _unitOfWork.InsuranceCompanies.GetByCodeAsync(dto.InsuranceCompanyCode);
            }

            if (company == null && !string.IsNullOrEmpty(dto.InsuranceCompanyName))
            {
                company = await _unitOfWork.InsuranceCompanies.GetByNameAsync(dto.InsuranceCompanyName);
            }

            if (company == null)
            {
                throw new InvalidOperationException(
                    $"Insurance company not found: {dto.InsuranceCompanyCode ?? dto.InsuranceCompanyName}");
            }

            return company;
        }

        private async Task<PolicyType> GetPolicyTypeAsync(ImportPolicyDto dto)
        {
            PolicyType? policyType = null;

            if (!string.IsNullOrEmpty(dto.PolicyTypeCode))
            {
                policyType = await _unitOfWork.PolicyTypes.GetByCodeAsync(dto.PolicyTypeCode);
            }

            if (policyType == null && !string.IsNullOrEmpty(dto.BranchCode))
            {
                policyType = await _unitOfWork.PolicyTypes.GetByCodeAsync(dto.BranchCode);
            }

            if (policyType == null)
            {
                throw new InvalidOperationException(
                    $"Policy type not found: {dto.PolicyTypeCode ?? dto.BranchCode}");
            }

            return policyType;
        }

        private async Task<Marketer?> GetOrCreateMarketerAsync(ImportPolicyDto dto, string userId)
        {
            if (string.IsNullOrEmpty(dto.MarketerCode))
            {
                return null;
            }

            var marketer = await _unitOfWork.Marketers.GetByCodeAsync(dto.MarketerCode);
            if (marketer == null)
            {
                // Auto-create marketer from import data
                marketer = new Marketer
                {
                    Code = dto.MarketerCode,
                    Name = dto.MarketerName ?? dto.MarketerCode,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = userId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _unitOfWork.Marketers.AddAsync(marketer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Auto-created marketer: {Code} - {Name}",
                    marketer.Code, marketer.Name);
            }

            return marketer;
        }

        private async Task<Currency> GetCurrencyAsync(string currencyCode)
        {
            var currency = await _unitOfWork.Currencies.GetByCodeAsync(currencyCode);
            if (currency == null)
            {
                throw new InvalidOperationException($"Currency not found: {currencyCode}");
            }

            return currency;
        }

        private async Task<Vehicle?> GetOrCreateVehicleAsync(ImportPolicyDto dto, int customerId, string userId)
        {
            if (string.IsNullOrEmpty(dto.PlateNumber))
            {
                return null;
            }

            var vehicle = await _unitOfWork.Vehicles.GetByPlateNumberAsync(dto.PlateNumber);
            if (vehicle != null)
            {
                return vehicle;
            }

            // Auto-create vehicle from import data
            vehicle = new Vehicle
            {
                CustomerId = customerId,
                PlateNumber = dto.PlateNumber,
                ModelYear = dto.VehicleYear,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedOn = DateTime.UtcNow
            };

            await _unitOfWork.Vehicles.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();

            return vehicle;
        }

        private DriverType? ParseDriverType(string? driverTypeText)
        {
            if (string.IsNullOrEmpty(driverTypeText))
            {
                return null;
            }

            var normalized = driverTypeText.ToLowerInvariant().Trim();

            if (normalized.Contains("single") || normalized.Contains("tek"))
            {
                return DriverType.Single;
            }

            if (normalized.Contains("any") || normalized.Contains("herhangi") || normalized.Contains("herkez"))
            {
                return DriverType.Any;
            }

            return null;
        }

        private PaymentMethod ParsePaymentMethod(string? paymentMethodText)
        {
            if (string.IsNullOrEmpty(paymentMethodText))
            {
                return PaymentMethod.Cash;
            }

            var normalized = paymentMethodText.ToLowerInvariant().Trim();

            if (normalized.Contains("cash") || normalized.Contains("nakit"))
                return PaymentMethod.Cash;

            if (normalized.Contains("credit") || normalized.Contains("kredi"))
                return PaymentMethod.CreditCard;

            if (normalized.Contains("transfer") || normalized.Contains("havale"))
                return PaymentMethod.BankTransfer;

            if (normalized.Contains("check") || normalized.Contains("çek"))
                return PaymentMethod.Cheque;

            return PaymentMethod.Cash;
        }

        private string GeneratePolicyNumber()
        {
            // Generate policy number in format: POL-YYYYMMDD-HHMMSSFFF
            return $"POL-{DateTime.Now:yyyyMMdd-HHmmssfff}";
        }
    }
}
