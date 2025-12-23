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
    // Helper class to track customer code generation state across the batch
    internal class CustomerCodeState
    {
        public string? BaseCode { get; set; }
        public int Counter { get; set; }
    }

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

                // Pre-load lookup data to avoid N+1 queries
                var allPolicyTypes = await _unitOfWork.PolicyTypes.GetAllAsync();
                var allCurrencies = await _unitOfWork.Currencies.GetAllAsync();
                var allCountries = await _unitOfWork.Countries.GetAllAsync();

                var policyTypeLookup = allPolicyTypes.ToDictionary(pt => pt.Code, StringComparer.OrdinalIgnoreCase);
                var currencyLookup = allCurrencies.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);
                var countryLookup = allCountries.ToDictionary(c => c.Code, StringComparer.OrdinalIgnoreCase);

                // Customer cache to avoid duplicate lookups
                var customerCache = new Dictionary<string, Customer>(StringComparer.OrdinalIgnoreCase);

                // Track generated customer codes in this batch to prevent duplicates
                var generatedCustomerCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // Customer code generation state for batch processing
                var codeState = new CustomerCodeState();

                // Vehicle cache to avoid duplicate lookups and FK constraint violations
                var vehicleCache = new Dictionary<string, Vehicle>(StringComparer.OrdinalIgnoreCase);

                // Policy cache to prevent duplicate PolicyNumber+InnerCode combinations AND to find original policies for endorsements
                var policyCache = new Dictionary<string, Policy>(StringComparer.OrdinalIgnoreCase);

                // Sort policies: Process InnerCode=000 (originals) first, then endorsements
                // This ensures original policies are in cache before endorsements need them
                var sortedPolicies = request.MappedPolicies
                    .OrderBy(p => p.OriginalImportData.InnerCode != "000" && p.OriginalImportData.InnerCode != null ? 1 : 0)
                    .ThenBy(p => p.RowNumber)
                    .ToList();

                _logger.LogInformation(
                    "Processing {Total} policies: {Originals} originals, {Endorsements} endorsements",
                    sortedPolicies.Count,
                    sortedPolicies.Count(p => p.OriginalImportData.InnerCode == "000" || p.OriginalImportData.InnerCode == null),
                    sortedPolicies.Count(p => p.OriginalImportData.InnerCode != "000" && p.OriginalImportData.InnerCode != null));

                // Import each policy (without SaveChanges per policy)
                foreach (var mappedPolicy in sortedPolicies)
                {
                    try
                    {
                        var policy = await ImportMappedPolicyAsync(
                            mappedPolicy,
                            request.UserId,
                            request.InsuranceCompanyId,
                            policyTypeLookup,
                            currencyLookup,
                            countryLookup,
                            customerCache,
                            generatedCustomerCodes,
                            vehicleCache,
                            policyCache,
                            codeState,
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

                // Batch commit ALL changes at once (huge performance improvement)
                if (result.SuccessCount > 0)
                {
                    _logger.LogInformation("Committing {Count} policies to database...", result.SuccessCount);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Successfully committed {Count} policies", result.SuccessCount);
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
            Dictionary<string, PolicyType> policyTypeLookup,
            Dictionary<string, Currency> currencyLookup,
            Dictionary<string, Country> countryLookup,
            Dictionary<string, Customer> customerCache,
            HashSet<string> generatedCustomerCodes,
            Dictionary<string, Vehicle> vehicleCache,
            Dictionary<string, Policy> policyCache,
            CustomerCodeState codeState,
            CancellationToken cancellationToken)
        {
            var dto = mappedPolicy.OriginalImportData;

            // Sigortalı (Insured Person) - Store as string in EnsuredEntity
            string? ensuredEntity = null;
            if (!string.IsNullOrEmpty(mappedPolicy.InsuredCustomerName))
            {
                ensuredEntity = $"{mappedPolicy.InsuredCustomerName} - {mappedPolicy.InsuredCustomerIdentifier}";
            }

            // Policy Owner (Cari) - determined by operator's selection
            Customer policyOwnerCustomer;
            if (mappedPolicy.PolicyOwnerSameAsInsured)
            {
                // Customer from Excel data becomes the policy owner
                policyOwnerCustomer = await GetOrCreateCustomerAsync(dto, countryLookup, customerCache, generatedCustomerCodes, codeState, userId, cancellationToken);
            }
            else if (mappedPolicy.CreateNewPolicyOwner)
            {
                // Check if we've already created this customer in this batch
                if (!string.IsNullOrEmpty(mappedPolicy.PolicyOwnerIdentifier) &&
                    customerCache.TryGetValue(mappedPolicy.PolicyOwnerIdentifier, out var cachedCustomer))
                {
                    _logger.LogInformation(
                        "Using cached customer for policy owner: {CustomerCode} - {Name}",
                        cachedCustomer.CustomerCode,
                        cachedCustomer.FirstName + " " + cachedCustomer.LastName);
                    policyOwnerCustomer = cachedCustomer;
                }
                else
                {
                    // Not in cache, create new customer
                    policyOwnerCustomer = await CreateCustomerAsync(
                        mappedPolicy.PolicyOwnerName,
                        mappedPolicy.PolicyOwnerIdentifier,
                        mappedPolicy.PolicyOwnerPhone,
                        dto.CustomerCountryCode,
                        dto.CustomerIdType,
                        countryLookup,
                        customerCache,
                        generatedCustomerCodes,
                        codeState,
                        userId,
                        cancellationToken);
                }
            }
            else if (mappedPolicy.PolicyOwnerCustomerId.HasValue)
            {
                // Use selected existing customer (fetch from database to get object reference)
                policyOwnerCustomer = await _unitOfWork.Customers.GetByIdAsync(mappedPolicy.PolicyOwnerCustomerId.Value)
                    ?? throw new InvalidOperationException($"Selected customer not found: {mappedPolicy.PolicyOwnerCustomerId.Value}");
            }
            else
            {
                throw new InvalidOperationException(
                    "Policy owner customer must be specified");
            }

            // Get other required entities using lookups (no DB calls)
            var policyType = GetPolicyTypeFromLookup(dto.PolicyTypeCode, policyTypeLookup);
            var currency = GetCurrencyFromLookup(dto.CurrencyCode ?? "TRY", currencyLookup);
            var vehicle = await GetOrCreateVehicleAsync(dto, policyOwnerCustomer, vehicleCache, userId);

            // Determine PolicyNumber and InnerCode
            string policyNumber = dto.PolicyNumber ?? GeneratePolicyNumber();
            string innerCode = dto.InnerCode ?? "000";

            // Check for duplicate PolicyNumber+InnerCode+InsuranceCompanyId+PolicyTypeId combination
            // Same policy number can exist for different insurance companies AND different policy types
            // Example: Policy 0000038 can be both Konut (home) and Trafik (traffic) for the same company
            string policyKey = $"{insuranceCompanyId}|{policyType.Id}|{policyNumber}|{innerCode}";

            // Check if already in current batch
            if (policyCache.ContainsKey(policyKey))
            {
                _logger.LogWarning(
                    "Skipping duplicate policy in batch: InsuranceCompanyId={CompanyId}, PolicyTypeId={TypeId}, PolicyNumber={PolicyNumber}, InnerCode={InnerCode}",
                    insuranceCompanyId,
                    policyType.Id,
                    policyNumber,
                    innerCode);
                throw new InvalidOperationException(
                    $"Duplicate policy in import: PolicyNumber={policyNumber}, InnerCode={innerCode}, PolicyType={policyType.Name} for insurance company {insuranceCompanyId}. This policy already exists in the current batch.");
            }

            // Check if already exists in database (for the same insurance company and policy type)
            var existingPolicy = await _unitOfWork.Policies.GetByPolicyNumberAndInnerCodeAsync(policyNumber, innerCode);
            if (existingPolicy != null && existingPolicy.InsuranceCompanyId == insuranceCompanyId && existingPolicy.PolicyTypeId == policyType.Id)
            {
                _logger.LogWarning(
                    "Skipping duplicate policy in database: InsuranceCompanyId={CompanyId}, PolicyTypeId={TypeId}, PolicyNumber={PolicyNumber}, InnerCode={InnerCode}",
                    insuranceCompanyId,
                    policyType.Id,
                    policyNumber,
                    innerCode);
                throw new InvalidOperationException(
                    $"Duplicate policy: PolicyNumber={policyNumber}, InnerCode={innerCode}, PolicyType={policyType.Name} for insurance company {insuranceCompanyId} already exists in database.");
            }

            // Check for endorsements - look in batch cache first, then database
            Policy? originalPolicy = null;
            if (innerCode != "000" && !string.IsNullOrEmpty(policyNumber))
            {
                // First check if original policy is in the current batch
                // Endorsement must be for the same insurance company and policy type
                string originalPolicyKey = $"{insuranceCompanyId}|{policyType.Id}|{policyNumber}|000";
                if (policyCache.TryGetValue(originalPolicyKey, out originalPolicy))
                {
                    _logger.LogDebug(
                        "Found original policy in batch cache for endorsement: PolicyNumber={PolicyNumber}",
                        policyNumber);
                }
                else
                {
                    // Not in batch, check database
                    originalPolicy = await _unitOfWork.Policies.GetByPolicyNumberAsync(policyNumber);
                    if (originalPolicy == null)
                    {
                        throw new InvalidOperationException(
                            $"Original policy not found for endorsement: {policyNumber}. The original policy (InnerCode=000) must be imported before endorsements.");
                    }
                }
            }

            // Create policy
            var policy = new Policy
            {
                PolicyNumber = policyNumber,
                Customer = policyOwnerCustomer,  // Use navigation property for batch processing
                EnsuredEntity = ensuredEntity, // Sigortalı (insured person - stored as string)
                InsuranceCompanyId = insuranceCompanyId,
                PolicyTypeId = policyType.Id,
                Vehicle = vehicle, // Use navigation property for batch processing
                StartDate = dto.StartDate ?? DateTime.Today,
                EndDate = dto.EndDate ?? DateTime.Today.AddYears(1),
                PremiumAmount = dto.PremiumAmount,
                CommissionRate = dto.CommissionRate ?? 0,
                CommissionAmount = dto.CommissionAmount ?? (dto.PremiumAmount * (dto.CommissionRate ?? 0) / 100),
                Status = dto.Status,
                CurrencyId = currency.Id,
                Notes = dto.Notes,
                InnerCode = innerCode,
                StateType = dto.StateType,
                OriginalPolicy = originalPolicy, // Use navigation property for batch processing
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
            // NOTE: SaveChangesAsync removed - will be called once for all policies in batch

            // Add to cache for endorsement lookups and duplicate prevention
            policyCache[policyKey] = policy;
            _logger.LogDebug("Added policy to cache: Company={CompanyId}, Type={TypeId}, Policy={PolicyNumber}, InnerCode={InnerCode}",
                insuranceCompanyId, policyType.Id, policyNumber, innerCode);

            // Create initial payment if applicable
            await CreateInitialPaymentIfNeeded(policy, policyType, dto, currency.Id, userId, cancellationToken);

            return policy;
        }

        private async Task<Customer> GetOrCreateCustomerAsync(
            ImportPolicyDto dto,
            Dictionary<string, Country> countryLookup,
            Dictionary<string, Customer> customerCache,
            HashSet<string> generatedCustomerCodes,
            CustomerCodeState codeState,
            string userId,
            CancellationToken cancellationToken)
        {
            // Try cache first
            if (!string.IsNullOrEmpty(dto.CustomerIdentifier) && customerCache.ContainsKey(dto.CustomerIdentifier))
            {
                return customerCache[dto.CustomerIdentifier];
            }

            // Try to find existing customer by identifier (exact match)
            if (!string.IsNullOrEmpty(dto.CustomerIdentifier))
            {
                var customer = await _unitOfWork.Customers.GetByIdentificationNoAsync(dto.CustomerIdentifier);
                if (customer != null)
                {
                    customerCache[dto.CustomerIdentifier] = customer; // Cache it
                    return customer;
                }
            }

            // Customer not found - create new customer using existing logic
            var newCustomer = await CreateCustomerAsync(
                dto.CustomerName,
                dto.CustomerIdentifier,
                null, // No phone number from Excel import
                dto.CustomerCountryCode,
                dto.CustomerIdType,
                countryLookup,
                customerCache,
                generatedCustomerCodes,
                codeState,
                userId,
                cancellationToken);

            return newCustomer;
        }

        private async Task<Customer> CreateCustomerAsync(
            string? customerName,
            string? customerIdentifier,
            string? phoneNumber,
            string? countryCode,
            string? idType,
            Dictionary<string, Country> countryLookup,
            Dictionary<string, Customer> customerCache,
            HashSet<string> generatedCustomerCodes,
            CustomerCodeState codeState,
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

            // Generate customer code using batch-aware logic
            string customerCode;

            if (codeState.BaseCode == null)
            {
                // First customer in batch - get the next available code from generator
                customerCode = await _customerCodeGenerator.GenerateAsync();
                codeState.BaseCode = customerCode;
                codeState.Counter = 1;

                _logger.LogInformation(
                    "Starting batch customer code generation from: {CustomerCode}",
                    customerCode);
            }
            else
            {
                // Subsequent customers - increment from the base code
                customerCode = IncrementCustomerCode(codeState.BaseCode, codeState.Counter);
                codeState.Counter++;

                _logger.LogDebug(
                    "Generated batch customer code #{Counter}: {CustomerCode}",
                    codeState.Counter,
                    customerCode);
            }

            // Track this code to prevent duplicates
            generatedCustomerCodes.Add(customerCode);

            IdentificationType identificationType = IdentificationType.Passport;
            int? nationalityCountryId = null;

            if (!string.IsNullOrEmpty(countryCode) && countryLookup.TryGetValue(countryCode, out var country))
            {
                nationalityCountryId = country.Id;
                if (countryCode == "601" ||
                    country.NameTr.ToUpperInvariant().Contains("KKTC") ||
                    country.NameEn.ToUpperInvariant().Contains("KKTC"))
                {
                    identificationType = IdentificationType.IdCard;
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
                Phone = !string.IsNullOrEmpty(phoneNumber) ? phoneNumber : "0000000000",
                Status = CustomerStatus.Active,
                IdentificationType = identificationType,
                NationalityCountryId = nationalityCountryId,
                Gender = Gender.Male,
                CreatedBy = userId
            };

            await _unitOfWork.Customers.AddAsync(customer);
            // NOTE: SaveChangesAsync removed - will be called once for all policies in batch

            // Cache the new customer to prevent duplicates in the same batch
            if (!string.IsNullOrEmpty(customerIdentifier))
            {
                customerCache[customerIdentifier] = customer;
            }

            _logger.LogInformation("Created new customer: {CustomerCode} - {Name}", customerCode, customerName);

            return customer;
        }

        private PolicyType GetPolicyTypeFromLookup(string? policyTypeCode, Dictionary<string, PolicyType> policyTypeLookup)
        {
            if (string.IsNullOrEmpty(policyTypeCode))
            {
                throw new InvalidOperationException("Policy type code is required");
            }

            if (!policyTypeLookup.TryGetValue(policyTypeCode, out var policyType))
            {
                throw new InvalidOperationException($"Policy type not found: {policyTypeCode}");
            }

            return policyType;
        }

        private Currency GetCurrencyFromLookup(string currencyCode, Dictionary<string, Currency> currencyLookup)
        {
            if (!currencyLookup.TryGetValue(currencyCode, out var currency))
            {
                throw new InvalidOperationException($"Currency not found: {currencyCode}");
            }

            return currency;
        }

        private async Task<Vehicle?> GetOrCreateVehicleAsync(
            ImportPolicyDto dto,
            Customer customer,
            Dictionary<string, Vehicle> vehicleCache,
            string userId)
        {
            if (string.IsNullOrEmpty(dto.PlateNumber))
            {
                return null;
            }

            // Try cache first (plate number is the key)
            if (vehicleCache.TryGetValue(dto.PlateNumber, out var cachedVehicle))
            {
                _logger.LogDebug("Using cached vehicle: {PlateNumber}", dto.PlateNumber);
                return cachedVehicle;
            }

            // Try to find existing vehicle in database
            var vehicle = await _unitOfWork.Vehicles.GetByPlateNumberAsync(dto.PlateNumber);
            if (vehicle != null)
            {
                // Cache it for subsequent policies in this batch
                vehicleCache[dto.PlateNumber] = vehicle;
                return vehicle;
            }

            // Create new vehicle
            vehicle = new Vehicle
            {
                Customer = customer, // Use navigation property for batch processing
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
            // NOTE: SaveChangesAsync removed - will be called once for all policies in batch

            // Cache the new vehicle to prevent duplicates in the same batch
            vehicleCache[dto.PlateNumber] = vehicle;

            _logger.LogInformation("Created new vehicle: {PlateNumber}", dto.PlateNumber);

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
            // IMPORTANT: Traffic and KASKO policies MUST be fully paid by law (no outstanding amount allowed)
            bool isTrafficPolicy = policyType.Code?.Contains("TRAFİK", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Code?.Contains("TRAFIK", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Code?.Contains("TRAFFIC", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Name?.Contains("Trafik", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Name?.Contains("Traffic", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Category?.Contains("Trafik", StringComparison.OrdinalIgnoreCase) == true ||
                                   policyType.Category?.Contains("Traffic", StringComparison.OrdinalIgnoreCase) == true;

            bool isKaskoPolicy = policyType.Code?.Contains("KASKO", StringComparison.OrdinalIgnoreCase) == true ||
                                 policyType.Name?.Contains("Kasko", StringComparison.OrdinalIgnoreCase) == true ||
                                 policyType.Category?.Contains("Kasko", StringComparison.OrdinalIgnoreCase) == true;

            bool requiresFullPayment = isTrafficPolicy || isKaskoPolicy;

            decimal paymentAmount = 0;
            if (requiresFullPayment)
            {
                // Traffic/KASKO policies: ALWAYS create full payment for premium amount
                // This ensures zero outstanding balance as required by law
                paymentAmount = policy.PremiumAmount; // Use actual policy premium, not DTO

                _logger.LogInformation(
                    "Creating full payment for {PolicyType} policy {PolicyNumber}: {Amount}",
                    isTrafficPolicy ? "TRAFFIC" : "KASKO",
                    policy.PolicyNumber,
                    paymentAmount);
            }
            else if (dto.PaidAmount.HasValue && dto.PaidAmount.Value > 0)
            {
                // Non-traffic policies: use paid amount from import if provided
                paymentAmount = dto.PaidAmount.Value;
            }

            if (paymentAmount > 0)
            {
                var payment = new PolicyPayment
                {
                    Policy = policy, // Use navigation property for batch processing
                    Amount = paymentAmount,
                    PaymentDate = dto.PaymentDate ?? policy.StartDate, // Use start date if no payment date
                    PaymentMethod = ParsePaymentMethod(dto.PaymentMethod),
                    Status = PaymentStatus.Completed,
                    CurrencyId = currencyId,
                    Notes = requiresFullPayment
                        ? (isTrafficPolicy ? "Trafik poliçesi - Tam ödeme (Yasal gereklilik)" : "Kasko poliçesi - Tam ödeme (Yasal gereklilik)")
                        : "Initial payment from import",
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = userId,
                    ModifiedOn = DateTime.UtcNow
                };

                await _unitOfWork.PolicyPayments.AddAsync(payment);
                // NOTE: SaveChangesAsync removed - will be called once for all policies in batch

                _logger.LogDebug(
                    "Created payment of {Amount} for policy {PolicyNumber} (Type: {PolicyType})",
                    paymentAmount,
                    policy.PolicyNumber,
                    requiresFullPayment ? (isTrafficPolicy ? "Traffic" : "KASKO") : "Other");
            }
            else if (requiresFullPayment)
            {
                // This should never happen, but log as warning if it does
                _logger.LogWarning(
                    "{PolicyType} policy {PolicyNumber} has zero premium amount - no payment created!",
                    isTrafficPolicy ? "Traffic" : "KASKO",
                    policy.PolicyNumber);
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

        private string IncrementCustomerCode(string baseCode, int increment)
        {
            // Customer codes typically have format: PREFIX + YEAR + NUMBER
            // Example: MUS2025000001
            // We need to extract the numeric portion and increment it

            // Find where the numbers start (after the prefix)
            int numberStartIndex = -1;
            for (int i = baseCode.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(baseCode[i]))
                {
                    numberStartIndex = i + 1;
                    break;
                }
            }

            if (numberStartIndex == -1 || numberStartIndex >= baseCode.Length)
            {
                // Entire string is digits or no digits found, unusual case
                throw new InvalidOperationException($"Cannot parse customer code format: {baseCode}");
            }

            // Extract prefix and numeric portion
            string prefix = baseCode.Substring(0, numberStartIndex);
            string numericPart = baseCode.Substring(numberStartIndex);

            // Parse the number, add increment, and format with leading zeros
            if (!long.TryParse(numericPart, out long number))
            {
                throw new InvalidOperationException($"Cannot parse numeric portion of customer code: {baseCode}");
            }

            long newNumber = number + increment;
            string newNumericPart = newNumber.ToString().PadLeft(numericPart.Length, '0');

            return prefix + newNumericPart;
        }

        private string GeneratePolicyNumber()
        {
            return $"POL-{DateTime.Now:yyyyMMdd-HHmmssfff}";
        }
    }
}
