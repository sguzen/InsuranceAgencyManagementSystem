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

namespace IAMS.Application.Features.Policies.Commands.SyncPoliciesFromMySql
{
    public class SyncPoliciesFromMySqlCommandHandler : IRequestHandler<SyncPoliciesFromMySqlCommand, Result<PolicyImportResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMySqlPolicyImportService _mySqlImportService;
        private readonly ICustomerCodeGenerator _customerCodeGenerator;
        private readonly ILogger<SyncPoliciesFromMySqlCommandHandler> _logger;

        public SyncPoliciesFromMySqlCommandHandler(
            IUnitOfWork unitOfWork,
            IMySqlPolicyImportService mySqlImportService,
            ICustomerCodeGenerator customerCodeGenerator,
            ILogger<SyncPoliciesFromMySqlCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mySqlImportService = mySqlImportService;
            _customerCodeGenerator = customerCodeGenerator;
            _logger = logger;
        }

        public async Task<Result<PolicyImportResultDto>> Handle(
            SyncPoliciesFromMySqlCommand request,
            CancellationToken cancellationToken)
        {
            ImportHistory? importHistory = null;

            try
            {
                // Validate insurance company exists before proceeding
                var insuranceCompany = await _unitOfWork.InsuranceCompanies.GetByIdAsync(request.InsuranceCompanyId);
                if (insuranceCompany == null)
                {
                    return Result<PolicyImportResultDto>.Failure(
                        $"Insurance company not found with ID: {request.InsuranceCompanyId}", (List<string>?)null);
                }

                // Use externally-provided configuration (from background import pipeline)
                // or fall back to looking up from tenant database
                ImportConfiguration? importConfig = request.ExternalConfiguration;
                if (importConfig == null)
                {
                    var configurations = await _unitOfWork.ImportConfigurations.GetByInsuranceCompanyIdAsync(request.InsuranceCompanyId);
                    importConfig = configurations.FirstOrDefault(c =>
                        c.SourceType == ImportSourceType.DatabaseImport && c.IsActive);
                }

                if (importConfig == null)
                {
                    return Result<PolicyImportResultDto>.Failure(
                        $"No active MySQL import configuration found for insurance company '{insuranceCompany.Name}' (ID: {request.InsuranceCompanyId}). " +
                        "Please create an ImportConfiguration with SourceType=DatabaseImport for this company.", (List<string>?)null);
                }

                var agencyCode = importConfig.ApiKey?.Trim() ?? "unknown";

                _logger.LogInformation(
                    "Starting MySQL database sync: Config={ConfigName}, Agency={AgencyCode}, DateRange={StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}",
                    importConfig.Name, agencyCode, request.StartDate, request.EndDate);

                // Create import history record
                importHistory = new ImportHistory
                {
                    SourceType = ImportSourceType.DatabaseImport,
                    InsuranceCompanyId = request.InsuranceCompanyId,
                    StartedAt = DateTime.UtcNow,
                    Status = "InProgress",
                    ImportedBy = request.UserId,
                    Notes = $"MySQL sync: Config={importConfig.Name}, Agency={agencyCode}, Range={request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}",
                    CreatedBy = request.UserId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = request.UserId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _unitOfWork.ImportHistories.AddAsync(importHistory);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Fetch policies from MySQL using the per-company configuration
                var policies = await _mySqlImportService.FetchPoliciesAsync(
                    importConfig,
                    request.StartDate,
                    request.EndDate,
                    cancellationToken);

                importHistory.TotalRecords = policies.Count;
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Fetched {Count} policies from MySQL database", policies.Count);

                // Process each policy through the import pipeline
                var result = new PolicyImportResultDto
                {
                    TotalRows = policies.Count
                };

                foreach (var policyDto in policies)
                {
                    try
                    {
                        var policy = await ImportSinglePolicyAsync(
                            policyDto, request.UserId, request.InsuranceCompanyId, cancellationToken);

                        result.SuccessCount++;
                        result.ImportedPolicyIds.Add(policy.Id);
                        importHistory.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        var innerMsg = ex.InnerException?.Message ?? ex.Message;
                        _logger.LogWarning(ex,
                            "Error importing policy from MySQL: Row={RowNumber}, PolicyNumber={PolicyNumber}, BranchCode={BranchCode}, Error={Error}",
                            policyDto.RowNumber, policyDto.PolicyNumber, policyDto.BranchCode, innerMsg);

                        result.FailureCount++;
                        importHistory.FailureCount++;

                        result.Errors.Add(new PolicyImportError
                        {
                            RowNumber = policyDto.RowNumber,
                            PolicyNumber = policyDto.PolicyNumber ?? "Unknown",
                            ErrorMessage = innerMsg
                        });

                        // Detach any pending tracked entities to prevent cascading failures
                        _unitOfWork.DetachAllEntities();
                        // Re-attach importHistory so we can continue updating it
                        _unitOfWork.AttachEntity(importHistory);
                    }
                }

                // Update import history
                importHistory.MarkAsCompleted(result.FailureCount == 0 ? "Success" : "PartialSuccess");
                importHistory.ModifiedBy = request.UserId;
                importHistory.ModifiedOn = DateTime.UtcNow;

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (Exception saveEx)
                {
                    _logger.LogError(saveEx, "Failed to save import history completion status");
                }

                _logger.LogInformation(
                    "MySQL sync completed. Total: {Total}, Success: {Success}, Failure: {Failure}",
                    result.TotalRows, result.SuccessCount, result.FailureCount);

                return Result<PolicyImportResultDto>.Success(result);
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                _logger.LogError(ex, "Error during MySQL database sync: {Error}", innerMsg);

                if (importHistory != null)
                {
                    try
                    {
                        importHistory.MarkAsCompleted("Failed");
                        importHistory.ErrorMessages = innerMsg;
                        importHistory.ModifiedBy = request.UserId;
                        importHistory.ModifiedOn = DateTime.UtcNow;
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                    catch (Exception saveEx)
                    {
                        _logger.LogError(saveEx, "Failed to save import history failure status");
                    }
                }

                return Result<PolicyImportResultDto>.Failure($"MySQL sync failed: {innerMsg}", (List<string>?)null);
            }
        }

        private async Task<Policy> ImportSinglePolicyAsync(
            ImportPolicyDto dto, string userId, int insuranceCompanyId, CancellationToken cancellationToken)
        {
            var customer = await GetOrCreateCustomerAsync(dto, userId);
            var policyType = await GetPolicyTypeAsync(dto);
            var currency = await GetCurrencyAsync(dto.CurrencyCode ?? "TRY");
            var vehicle = await GetOrCreateVehicleAsync(dto, customer.Id, userId);

            // Check if this is an endorsement
            Policy? originalPolicy = null;
            if (dto.InnerCode != "000" && !string.IsNullOrEmpty(dto.PolicyNumber))
            {
                originalPolicy = await _unitOfWork.Policies.GetByPolicyNumberAsync(dto.PolicyNumber);
            }

            // Build EnsuredEntity string (Sigortalı) - same pattern as CSV import
            // The insured person is stored as text; the Customer (Cari Kart) is the policy owner
            string? ensuredEntity = null;
            if (!string.IsNullOrEmpty(dto.CustomerName))
            {
                ensuredEntity = !string.IsNullOrEmpty(dto.CustomerIdentifier)
                    ? $"{dto.CustomerName} - {dto.CustomerIdentifier}"
                    : dto.CustomerName;
            }

            var policy = new Policy
            {
                PolicyNumber = dto.PolicyNumber ?? $"POL-{DateTime.Now:yyyyMMdd-HHmmssfff}",
                CustomerId = customer.Id,
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
                EnsuredEntity = ensuredEntity,
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

            // Auto-create payment for traffic and kasko policies (legally required to be fully paid)
            await CreateAutoPaymentIfRequiredAsync(policy, policyType, currency.Id, userId, cancellationToken);

            return policy;
        }

        /// <summary>
        /// Traffic (Trafik) and Kasko policies are legally required to be fully paid.
        /// Automatically creates a completed payment record for these policy types.
        /// </summary>
        private async Task CreateAutoPaymentIfRequiredAsync(
            Policy policy, PolicyType policyType, int currencyId, string userId, CancellationToken cancellationToken)
        {
            bool isTrafficPolicy =
                policyType.Code?.Contains("TRAFİK", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Code?.Contains("TRAFIK", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Code?.Contains("TRAFFIC", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Name?.Contains("Trafik", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Name?.Contains("Traffic", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Category?.Contains("Trafik", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Category?.Contains("Traffic", StringComparison.OrdinalIgnoreCase) == true;

            bool isKaskoPolicy =
                policyType.Code?.Contains("KASKO", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Name?.Contains("Kasko", StringComparison.OrdinalIgnoreCase) == true ||
                policyType.Category?.Contains("Kasko", StringComparison.OrdinalIgnoreCase) == true;

            if (!isTrafficPolicy && !isKaskoPolicy)
                return;

            if (policy.PremiumAmount <= 0)
                return;

            var payment = new PolicyPayment
            {
                PolicyId = policy.Id,
                Amount = policy.PremiumAmount,
                PaymentDate = policy.StartDate,
                PaymentMethod = PaymentMethod.BankTransfer,
                Status = PaymentStatus.Completed,
                CurrencyId = currencyId,
                Notes = isTrafficPolicy
                    ? "Trafik poliçesi - Tam ödeme (Yasal gereklilik)"
                    : "Kasko poliçesi - Tam ödeme (Yasal gereklilik)",
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedOn = DateTime.UtcNow
            };

            await _unitOfWork.PolicyPayments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Auto-created payment for {PolicyType} policy {PolicyNumber}: {Amount}",
                isTrafficPolicy ? "traffic" : "kasko", policy.PolicyNumber, policy.PremiumAmount);
        }

        private async Task<Customer> GetOrCreateCustomerAsync(ImportPolicyDto dto, string userId)
        {
            if (!string.IsNullOrEmpty(dto.CustomerIdentifier))
            {
                var customer = await _unitOfWork.Customers.GetByIdentificationNoAsync(dto.CustomerIdentifier);
                if (customer != null)
                    return customer;
            }

            if (string.IsNullOrEmpty(dto.CustomerName) || string.IsNullOrEmpty(dto.CustomerIdentifier))
            {
                throw new InvalidOperationException(
                    $"Cannot create customer: missing name or identifier. Row: {dto.RowNumber}");
            }

            var customerType = dto.CustomerIdType?.ToUpperInvariant() is "MŞ" or "MS"
                ? CustomerType.Corporate
                : CustomerType.Individual;

            var nameParts = dto.CustomerName.Split(new[] { ' ' }, 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : (customerType == CustomerType.Corporate ? "" : firstName);

            var customerCode = await _customerCodeGenerator.GenerateAsync();

            IdentificationType identificationType = IdentificationType.Passport;
            int? nationalityCountryId = null;

            if (!string.IsNullOrEmpty(dto.CustomerCountryCode))
            {
                var country = await _unitOfWork.Countries.GetByCodeAsync(dto.CustomerCountryCode);
                if (country != null)
                {
                    nationalityCountryId = country.Id;
                    if (dto.CustomerCountryCode == "601" ||
                        country.NameTr.ToUpperInvariant().Contains("KKTC") ||
                        country.NameEn.ToUpperInvariant().Contains("KKTC"))
                    {
                        identificationType = IdentificationType.IdCard;
                    }
                }
            }

            var newCustomer = new Customer
            {
                CustomerCode = customerCode,
                FirstName = firstName,
                LastName = lastName,
                Type = customerType,
                IdentificationNumber = dto.CustomerIdentifier,
                Email = $"noemail_{Guid.NewGuid():N}@temp.com",
                Phone = "0000000000",
                Status = CustomerStatus.Active,
                IdentificationType = identificationType,
                NationalityCountryId = nationalityCountryId,
                Gender = Gender.Male,
                CreatedBy = userId
            };

            await _unitOfWork.Customers.AddAsync(newCustomer);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created new customer: {CustomerId} - {Name}",
                newCustomer.Id, dto.CustomerName);

            return newCustomer;
        }

        private async Task<PolicyType> GetPolicyTypeAsync(ImportPolicyDto dto)
        {
            if (!string.IsNullOrEmpty(dto.PolicyTypeCode))
            {
                var policyType = await _unitOfWork.PolicyTypes.GetByCodeAsync(dto.PolicyTypeCode);
                if (policyType != null)
                    return policyType;
            }

            throw new InvalidOperationException(
                $"Policy type not found: {dto.PolicyTypeCode ?? dto.PolicyTypeName}");
        }

        private async Task<Currency> GetCurrencyAsync(string currencyCode)
        {
            var currency = await _unitOfWork.Currencies.GetByCodeAsync(currencyCode);
            if (currency == null)
                throw new InvalidOperationException($"Currency not found: {currencyCode}");
            return currency;
        }

        private async Task<Vehicle?> GetOrCreateVehicleAsync(ImportPolicyDto dto, int customerId, string userId)
        {
            if (string.IsNullOrEmpty(dto.PlateNumber))
                return null;

            var vehicle = await _unitOfWork.Vehicles.GetByPlateNumberAsync(dto.PlateNumber);
            if (vehicle != null)
                return vehicle;

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

        private static DriverType? ParseDriverType(string? driverTypeText)
        {
            if (string.IsNullOrEmpty(driverTypeText))
                return null;

            var normalized = driverTypeText.ToLowerInvariant().Trim();

            if (normalized.Contains("name") || normalized.Contains("single") || normalized.Contains("tek"))
                return DriverType.Single;

            if (normalized.Contains("any") || normalized.Contains("herhangi") || normalized.Contains("herkez"))
                return DriverType.Any;

            return null;
        }
    }
}
