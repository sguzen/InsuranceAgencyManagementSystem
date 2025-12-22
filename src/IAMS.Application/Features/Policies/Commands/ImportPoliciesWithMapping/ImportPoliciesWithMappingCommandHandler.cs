using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using IAMS.Application.Services;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.DTOs.Policy;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Commands.ImportPoliciesWithMapping
{
    public class ImportPoliciesWithMappingCommandHandler
        : IRequestHandler<ImportPoliciesWithMappingCommand, Result<PolicyImportResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerCodeGenerator _customerCodeGenerator;
        private readonly ILogger<ImportPoliciesWithMappingCommandHandler> _logger;

        public ImportPoliciesWithMappingCommandHandler(
            IUnitOfWork unitOfWork,
            ICustomerCodeGenerator customerCodeGenerator,
            ILogger<ImportPoliciesWithMappingCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _customerCodeGenerator = customerCodeGenerator;
            _logger = logger;
        }

        public async Task<Result<PolicyImportResultDto>> Handle(
            ImportPoliciesWithMappingCommand request,
            CancellationToken cancellationToken)
        {
            var result = new PolicyImportResultDto
            {
                TotalRows = request.MappedPolicies.Count
            };

            try
            {
                // Validate all policies are mapped
                var unmapped = request.MappedPolicies.Where(p => !p.IsValid).ToList();
                if (unmapped.Any())
                {
                    return Result<PolicyImportResultDto>.Failure(
                        $"{unmapped.Count} policies are not properly mapped",
                        (List<string>?)null);
                }

                _logger.LogInformation(
                    "Starting import of {Count} mapped policies for insurance company {InsuranceCompanyId}",
                    request.MappedPolicies.Count,
                    request.InsuranceCompanyId);

                // Import each policy
                foreach (var mappedPolicy in request.MappedPolicies)
                {
                    try
                    {
                        var policy = await ImportMappedPolicyAsync(
                            mappedPolicy,
                            request.UserId,
                            request.InsuranceCompanyId,
                            cancellationToken);

                        result.SuccessCount++;
                        result.ImportedPolicyIds.Add(policy.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Error importing policy at row {RowNumber}: {PolicyNumber}",
                            mappedPolicy.RowNumber,
                            mappedPolicy.PolicyNumber);

                        result.FailureCount++;
                        result.Errors.Add(new PolicyImportError
                        {
                            RowNumber = mappedPolicy.RowNumber,
                            PolicyNumber = mappedPolicy.PolicyNumber ?? "Unknown",
                            ErrorMessage = ex.Message
                        });
                    }
                }

                _logger.LogInformation(
                    "Import completed. Success: {Success}, Failure: {Failure}",
                    result.SuccessCount,
                    result.FailureCount);

                return Result<PolicyImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during policy import with mapping");
                return Result<PolicyImportResultDto>.Failure(
                    $"Import failed: {ex.Message}",
                    (List<string>?)null);
            }
        }

        private async Task<Policy> ImportMappedPolicyAsync(
            PolicyImportPreviewDto mappedPolicy,
            string userId,
            int insuranceCompanyId,
            CancellationToken cancellationToken)
        {
            var dto = mappedPolicy.OriginalImportData;

            // Sigortalı (Insured Customer) - ALWAYS auto-created/matched from Excel data
            // Uses existing GetOrCreateCustomerAsync logic (lookup by ID, create if not exists)
            var insuredCustomer = await GetOrCreateCustomerAsync(dto, userId, cancellationToken);
            int insuredCustomerId = insuredCustomer.Id;

            // Policy Owner - determined by operator's selection
            int policyOwnerCustomerId;
            if (mappedPolicy.PolicyOwnerSameAsInsured)
            {
                // Same customer pays and is insured
                policyOwnerCustomerId = insuredCustomerId;
            }
            else if (mappedPolicy.CreateNewPolicyOwner)
            {
                // Create new policy owner customer
                var ownerCustomer = await CreateCustomerAsync(
                    mappedPolicy.PolicyOwnerName,
                    mappedPolicy.PolicyOwnerIdentifier,
                    dto.CustomerCountryCode,
                    dto.CustomerIdType,
                    userId,
                    cancellationToken);
                policyOwnerCustomerId = ownerCustomer.Id;
            }
            else if (mappedPolicy.PolicyOwnerCustomerId.HasValue)
            {
                // Use selected existing customer
                policyOwnerCustomerId = mappedPolicy.PolicyOwnerCustomerId.Value;
            }
            else
            {
                throw new InvalidOperationException(
                    "Policy owner customer must be specified");
            }

            // Get other required entities
            var policyType = await GetPolicyTypeAsync(dto.PolicyTypeCode);
            var currency = await GetCurrencyAsync(dto.CurrencyCode ?? "TRY");
            var vehicle = await GetOrCreateVehicleAsync(dto, insuredCustomerId, userId);

            // Check for endorsements
            Policy? originalPolicy = null;
            if (dto.InnerCode != "000" && !string.IsNullOrEmpty(dto.PolicyNumber))
            {
                originalPolicy = await _unitOfWork.Policies.GetByPolicyNumberAsync(dto.PolicyNumber);
                if (originalPolicy == null)
                {
                    throw new InvalidOperationException(
                        $"Original policy not found for endorsement: {dto.PolicyNumber}");
                }
            }

            // Create policy
            var policy = new Policy
            {
                PolicyNumber = dto.PolicyNumber ?? GeneratePolicyNumber(),
                CustomerId = policyOwnerCustomerId,  // Policy owner (who pays)
                InsuredCustomerId = insuredCustomerId, // Insured customer (who is insured)
                InsuranceCompanyId = insuranceCompanyId,
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
                InnerCode = dto.InnerCode,
                StateType = dto.StateType,
                OriginalPolicyId = originalPolicy?.Id,
                BranchCode = dto.BranchCode,
                DriverAge = dto.DriverAge,
                DriverType = ParseDriverType(dto.DriverTypeText),
                Marketer = dto.Marketer,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedOn = DateTime.UtcNow
            };

            await _unitOfWork.Policies.AddAsync(policy);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create initial payment if applicable
            await CreateInitialPaymentIfNeeded(policy, policyType, dto, currency.Id, userId, cancellationToken);

            return policy;
        }

        private async Task<Customer> GetOrCreateCustomerAsync(
            ImportPolicyDto dto,
            string userId,
            CancellationToken cancellationToken)
        {
            // Try to find existing customer by identifier (exact match)
            if (!string.IsNullOrEmpty(dto.CustomerIdentifier))
            {
                var customer = await _unitOfWork.Customers.GetByIdentificationNoAsync(dto.CustomerIdentifier);
                if (customer != null)
                {
                    return customer;
                }
            }

            // Customer not found - create new customer using existing logic
            return await CreateCustomerAsync(
                dto.CustomerName,
                dto.CustomerIdentifier,
                dto.CustomerCountryCode,
                dto.CustomerIdType,
                userId,
                cancellationToken);
        }

        private async Task<Customer> CreateCustomerAsync(
            string? customerName,
            string? customerIdentifier,
            string? countryCode,
            string? idType,
            string userId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerIdentifier))
            {
                throw new InvalidOperationException("Customer name and identifier are required");
            }

            var customerType = idType?.ToUpperInvariant() == "MŞ" || idType?.ToUpperInvariant() == "MS"
                ? CustomerType.Corporate
                : CustomerType.Individual;

            var nameParts = customerName.Split(new[] { ' ' }, 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : (customerType == CustomerType.Corporate ? "" : firstName);

            var customerCode = await _customerCodeGenerator.GenerateAsync();

            IdentificationType identificationType = IdentificationType.Passport;
            int? nationalityCountryId = null;

            if (!string.IsNullOrEmpty(countryCode))
            {
                var country = await _unitOfWork.Countries.GetByCodeAsync(countryCode);
                if (country != null)
                {
                    nationalityCountryId = country.Id;
                    if (countryCode == "601" ||
                        country.NameTr.ToUpperInvariant().Contains("KKTC") ||
                        country.NameEn.ToUpperInvariant().Contains("KKTC"))
                    {
                        identificationType = IdentificationType.IdCard;
                    }
                }
            }

            var customer = new Customer
            {
                CustomerCode = customerCode,
                FirstName = firstName,
                LastName = lastName,
                Type = customerType,
                IdentificationNumber = customerIdentifier,
                Email = $"noemail_{customerIdentifier}@temp.com",
                Phone = "0000000000",
                Status = CustomerStatus.Active,
                IdentificationType = identificationType,
                NationalityCountryId = nationalityCountryId,
                Gender = Gender.Male,
                CreatedBy = userId
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new customer: {CustomerCode} - {Name}", customerCode, customerName);

            return customer;
        }

        private async Task<PolicyType> GetPolicyTypeAsync(string? policyTypeCode)
        {
            if (string.IsNullOrEmpty(policyTypeCode))
            {
                throw new InvalidOperationException("Policy type code is required");
            }

            var policyType = await _unitOfWork.PolicyTypes.GetByCodeAsync(policyTypeCode);
            if (policyType == null)
            {
                throw new InvalidOperationException($"Policy type not found: {policyTypeCode}");
            }

            return policyType;
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

        private async Task<Vehicle?> GetOrCreateVehicleAsync(
            ImportPolicyDto dto,
            int customerId,
            string userId)
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

            vehicle = new Vehicle
            {
                CustomerId = customerId,
                PlateNumber = dto.PlateNumber,
                BrandName = dto.VehicleBrand,
                ModelName = dto.VehicleModel,
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

        private async Task CreateInitialPaymentIfNeeded(
            Policy policy,
            PolicyType policyType,
            ImportPolicyDto dto,
            int currencyId,
            string userId,
            CancellationToken cancellationToken)
        {
            bool isTrafficPolicy = policyType.Category?.Contains("Trafik", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Category?.Contains("Traffic", StringComparison.OrdinalIgnoreCase) == true;

            decimal paymentAmount = 0;
            if (isTrafficPolicy)
            {
                paymentAmount = dto.PremiumAmount;
            }
            else if (dto.PaidAmount.HasValue && dto.PaidAmount.Value > 0)
            {
                paymentAmount = dto.PaidAmount.Value;
            }

            if (paymentAmount > 0)
            {
                var payment = new PolicyPayment
                {
                    PolicyId = policy.Id,
                    Amount = paymentAmount,
                    PaymentDate = dto.PaymentDate ?? DateTime.Today,
                    PaymentMethod = ParsePaymentMethod(dto.PaymentMethod),
                    Status = PaymentStatus.Completed,
                    CurrencyId = currencyId,
                    Notes = isTrafficPolicy ? "Auto-payment for traffic policy" : "Initial payment from import",
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = userId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _unitOfWork.PolicyPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }

        private DriverType? ParseDriverType(string? driverTypeText)
        {
            if (string.IsNullOrEmpty(driverTypeText))
                return null;

            var normalized = driverTypeText.ToLowerInvariant().Trim();

            if (normalized.Contains("single") || normalized.Contains("tek"))
                return DriverType.Single;

            if (normalized.Contains("any") || normalized.Contains("herhangi") || normalized.Contains("herkez"))
                return DriverType.Any;

            return null;
        }

        private PaymentMethod ParsePaymentMethod(string? paymentMethodText)
        {
            if (string.IsNullOrEmpty(paymentMethodText))
                return PaymentMethod.Cash;

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
            return $"POL-{DateTime.Now:yyyyMMdd-HHmmssfff}";
        }
    }
}
