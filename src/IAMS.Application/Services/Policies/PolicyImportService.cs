using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IAMS.Application.Services.Policies
{
    /// <summary>
    /// Service for importing policies and endorsements from Excel files
    /// This implementation uses ClosedXML for Excel file processing
    /// </summary>
    public class PolicyImportService : IPolicyImportService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IInsuranceCompanyRepository _insuranceCompanyRepository;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IMarketerRepository _marketerRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IPolicyPaymentRepository _policyPaymentRepository;
        private readonly ILogger<PolicyImportService> _logger;

        public PolicyImportService(
            IPolicyRepository policyRepository,
            ICustomerRepository customerRepository,
            IInsuranceCompanyRepository insuranceCompanyRepository,
            IPolicyTypeRepository policyTypeRepository,
            IMarketerRepository marketerRepository,
            IVehicleRepository vehicleRepository,
            ICurrencyRepository currencyRepository,
            IPolicyPaymentRepository policyPaymentRepository,
            ILogger<PolicyImportService> logger)
        {
            _policyRepository = policyRepository;
            _customerRepository = customerRepository;
            _insuranceCompanyRepository = insuranceCompanyRepository;
            _policyTypeRepository = policyTypeRepository;
            _marketerRepository = marketerRepository;
            _vehicleRepository = vehicleRepository;
            _currencyRepository = currencyRepository;
            _policyPaymentRepository = policyPaymentRepository;
            _logger = logger;
        }

        public async Task<PolicyImportResultDto> ImportFromExcelAsync(string filePath, string userId)
        {
            var result = new PolicyImportResultDto();

            try
            {
                // Parse Excel file
                var policies = await ParseExcelAsync(filePath);
                result.TotalRows = policies.Count;

                // Import each policy
                foreach (var policyDto in policies)
                {
                    try
                    {
                        await ImportSinglePolicyAsync(policyDto, userId);
                        result.SuccessCount++;
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
                _logger.LogError(ex, "Error reading Excel file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to read Excel file: {ex.Message}", ex);
            }

            return result;
        }

        public async Task<PolicyImportResultDto> ImportFromStreamAsync(Stream stream, string userId)
        {
            // Create a temporary file from the stream
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fileStream = File.Create(tempFile))
                {
                    await stream.CopyToAsync(fileStream);
                }

                return await ImportFromExcelAsync(tempFile, userId);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public async Task<PolicyImportResultDto> ValidateExcelAsync(string filePath)
        {
            var result = new PolicyImportResultDto();

            try
            {
                var policies = await ParseExcelAsync(filePath);
                result.TotalRows = policies.Count;

                foreach (var policyDto in policies)
                {
                    var validationErrors = await ValidatePolicyDtoAsync(policyDto);
                    if (validationErrors.Any())
                    {
                        result.FailureCount++;
                        result.Errors.AddRange(validationErrors);
                    }
                    else
                    {
                        result.SuccessCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Excel file: {FilePath}", filePath);
                throw;
            }

            return result;
        }

        public Task<List<ImportPolicyDto>> ParseExcelAsync(string filePath)
        {
            // This will be implemented with ClosedXML
            // For now, return a placeholder implementation
            // The actual implementation will read the Excel columns and map them to ImportPolicyDto

            throw new NotImplementedException(
                "Excel parsing requires ClosedXML library. " +
                "Please add PackageReference to ClosedXML in IAMS.Application.csproj and implement ParseExcelAsync method.");
        }

        private async Task<Policy> ImportSinglePolicyAsync(ImportPolicyDto dto, string userId)
        {
            // Lookup or create related entities
            var customer = await GetOrCreateCustomerAsync(dto);
            var insuranceCompany = await GetInsuranceCompanyAsync(dto);
            var policyType = await GetPolicyTypeAsync(dto);
            var marketer = await GetOrCreateMarketerAsync(dto);
            var currency = await GetCurrencyAsync(dto.CurrencyCode ?? "TRY");
            var vehicle = await GetOrCreateVehicleAsync(dto, customer.Id);

            // Check if this is an endorsement
            Policy? originalPolicy = null;
            string? endorsementNumber = null;

            if (dto.IsEndorsement && !string.IsNullOrEmpty(dto.PolicyNumber))
            {
                // Find the original policy
                originalPolicy = await _policyRepository.GetByPolicyNumberAsync(dto.PolicyNumber);
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
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userId,
                UpdatedDate = DateTime.UtcNow
            };

            // Save policy
            await _policyRepository.AddAsync(policy);
            await _policyRepository.SaveChangesAsync();

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
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.UtcNow
                };

                await _policyPaymentRepository.AddAsync(payment);
                await _policyPaymentRepository.SaveChangesAsync();
            }

            return policy;
        }

        private async Task<string> GenerateEndorsementNumberAsync(int originalPolicyId)
        {
            // Get all endorsements for this policy
            var endorsements = await _policyRepository.GetEndorsementsByOriginalPolicyIdAsync(originalPolicyId);

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

        private async Task<List<PolicyImportError>> ValidatePolicyDtoAsync(ImportPolicyDto dto)
        {
            var errors = new List<PolicyImportError>();

            if (string.IsNullOrWhiteSpace(dto.PolicyNumber))
            {
                errors.Add(new PolicyImportError
                {
                    RowNumber = dto.RowNumber,
                    PolicyNumber = dto.PolicyNumber ?? "",
                    FieldName = nameof(dto.PolicyNumber),
                    ErrorMessage = "Policy number is required"
                });
            }

            if (string.IsNullOrWhiteSpace(dto.CustomerIdentifier))
            {
                errors.Add(new PolicyImportError
                {
                    RowNumber = dto.RowNumber,
                    PolicyNumber = dto.PolicyNumber ?? "",
                    FieldName = nameof(dto.CustomerIdentifier),
                    ErrorMessage = "Customer identifier is required"
                });
            }

            if (dto.PremiumAmount <= 0)
            {
                errors.Add(new PolicyImportError
                {
                    RowNumber = dto.RowNumber,
                    PolicyNumber = dto.PolicyNumber ?? "",
                    FieldName = nameof(dto.PremiumAmount),
                    ErrorMessage = "Premium amount must be greater than zero"
                });
            }

            if (!dto.StartDate.HasValue || !dto.EndDate.HasValue)
            {
                errors.Add(new PolicyImportError
                {
                    RowNumber = dto.RowNumber,
                    PolicyNumber = dto.PolicyNumber ?? "",
                    FieldName = "Dates",
                    ErrorMessage = "Start date and end date are required"
                });
            }
            else if (dto.StartDate >= dto.EndDate)
            {
                errors.Add(new PolicyImportError
                {
                    RowNumber = dto.RowNumber,
                    PolicyNumber = dto.PolicyNumber ?? "",
                    FieldName = "Dates",
                    ErrorMessage = "Start date must be before end date"
                });
            }

            return errors;
        }

        private async Task<Customer> GetOrCreateCustomerAsync(ImportPolicyDto dto)
        {
            // Try to find existing customer by identifier
            if (!string.IsNullOrEmpty(dto.CustomerIdentifier))
            {
                var customer = await _customerRepository.GetByTaxOrIdentityNumberAsync(dto.CustomerIdentifier);
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
                company = await _insuranceCompanyRepository.GetByCodeAsync(dto.InsuranceCompanyCode);
            }

            if (company == null && !string.IsNullOrEmpty(dto.InsuranceCompanyName))
            {
                company = await _insuranceCompanyRepository.GetByNameAsync(dto.InsuranceCompanyName);
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
                policyType = await _policyTypeRepository.GetByCodeAsync(dto.PolicyTypeCode);
            }

            if (policyType == null && !string.IsNullOrEmpty(dto.BranchCode))
            {
                policyType = await _policyTypeRepository.GetByCodeAsync(dto.BranchCode);
            }

            if (policyType == null)
            {
                throw new InvalidOperationException(
                    $"Policy type not found: {dto.PolicyTypeCode ?? dto.BranchCode}");
            }

            return policyType;
        }

        private async Task<Marketer?> GetOrCreateMarketerAsync(ImportPolicyDto dto)
        {
            if (string.IsNullOrEmpty(dto.MarketerCode))
            {
                return null;
            }

            var marketer = await _marketerRepository.GetByCodeAsync(dto.MarketerCode);
            if (marketer == null)
            {
                // Auto-create marketer from import data
                marketer = new Marketer
                {
                    Code = dto.MarketerCode,
                    Name = dto.MarketerName ?? dto.MarketerCode,
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedDate = DateTime.UtcNow
                };

                await _marketerRepository.AddAsync(marketer);
                await _marketerRepository.SaveChangesAsync();

                _logger.LogInformation("Auto-created marketer: {Code} - {Name}",
                    marketer.Code, marketer.Name);
            }

            return marketer;
        }

        private async Task<Currency> GetCurrencyAsync(string currencyCode)
        {
            var currency = await _currencyRepository.GetByCodeAsync(currencyCode);
            if (currency == null)
            {
                throw new InvalidOperationException($"Currency not found: {currencyCode}");
            }

            return currency;
        }

        private async Task<Vehicle?> GetOrCreateVehicleAsync(ImportPolicyDto dto, int customerId)
        {
            if (string.IsNullOrEmpty(dto.PlateNumber))
            {
                return null;
            }

            var vehicle = await _vehicleRepository.GetByPlateNumberAsync(dto.PlateNumber);
            if (vehicle != null)
            {
                return vehicle;
            }

            // Auto-create vehicle from import data
            // This is a simplified version - you may want to lookup brand/model IDs
            vehicle = new Vehicle
            {
                CustomerId = customerId,
                PlateNumber = dto.PlateNumber,
                Year = dto.VehicleYear,
                // Brand and Model would need to be looked up or created
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = "System",
                UpdatedDate = DateTime.UtcNow
            };

            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();

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
                return PaymentMethod.Check;

            return PaymentMethod.Cash;
        }

        private string GeneratePolicyNumber()
        {
            // This should use PolicyNumberGenerator service
            // For now, return a simple timestamp-based number
            return $"POL-{DateTime.Now:yyyyMMddHHmmss}";
        }
    }
}
