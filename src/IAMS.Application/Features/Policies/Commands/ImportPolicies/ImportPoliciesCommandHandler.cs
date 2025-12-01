using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;

namespace IAMS.Application.Features.Policies.Commands.ImportPolicies
{
    public class ImportPoliciesCommandHandler : IRequestHandler<ImportPoliciesCommand, Result<PolicyImportResultDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImportPoliciesCommandHandler> _logger;

        public ImportPoliciesCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<ImportPoliciesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
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
                    result = await ImportFromStreamAsync(stream, request.UserId, cancellationToken);
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

        private async Task<PolicyImportResultDto> ImportFromStreamAsync(Stream stream, string userId, CancellationToken cancellationToken)
        {
            // Create a temporary file from the stream
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fileStream = File.Create(tempFile))
                {
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                return await ImportFromExcelAsync(tempFile, userId, cancellationToken);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private async Task<PolicyImportResultDto> ImportFromExcelAsync(string filePath, string userId, CancellationToken cancellationToken)
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
                _logger.LogError(ex, "Error reading Excel file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to read Excel file: {ex.Message}", ex);
            }

            return result;
        }

        private Task<List<ImportPolicyDto>> ParseExcelAsync(string filePath)
        {
            var policies = new List<ImportPolicyDto>();

            try
            {
                using var workbook = new XLWorkbook(filePath);
                var worksheet = workbook.Worksheet(1); // First worksheet

                // Find the header row (first non-empty row)
                var headerRow = worksheet.FirstRowUsed();
                if (headerRow == null)
                {
                    throw new InvalidOperationException("Excel file is empty");
                }

                // Map column headers to indices
                var columnMap = BuildColumnMap(headerRow);

                // Process each data row
                var currentRow = headerRow.RowBelow();
                int rowNumber = 2; // Start from row 2 (row 1 is header)

                while (currentRow != null && !currentRow.IsEmpty())
                {
                    try
                    {
                        var policy = ParseRow(currentRow, columnMap, rowNumber);
                        if (policy != null)
                        {
                            policies.Add(policy);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing row {RowNumber}: {Message}", rowNumber, ex.Message);
                        // Continue processing other rows
                    }

                    currentRow = currentRow.RowBelow();
                    rowNumber++;
                }

                _logger.LogInformation("Successfully parsed {Count} policies from Excel file", policies.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Excel file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to parse Excel file: {ex.Message}", ex);
            }

            return Task.FromResult(policies);
        }

        private Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
        {
            var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in headerRow.CellsUsed())
            {
                var headerText = cell.GetString().Trim();
                if (string.IsNullOrEmpty(headerText)) continue;

                // Normalize header text for easier matching
                var normalizedHeader = NormalizeHeaderText(headerText);
                columnMap[normalizedHeader] = cell.Address.ColumnNumber;

                // Also store original header
                columnMap[headerText] = cell.Address.ColumnNumber;
            }

            return columnMap;
        }

        private string NormalizeHeaderText(string text)
        {
            // Remove special characters and normalize Turkish characters
            return text.ToLowerInvariant()
                .Replace("ı", "i")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ş", "s")
                .Replace("ö", "o")
                .Replace("ç", "c")
                .Replace(".", "")
                .Replace(" ", "")
                .Replace("/", "")
                .Replace("-", "")
                .Trim();
        }

        private ImportPolicyDto? ParseRow(IXLRow row, Dictionary<string, int> columnMap, int rowNumber)
        {
            // Check if row has any data
            if (row.IsEmpty()) return null;

            var policy = new ImportPolicyDto
            {
                RowNumber = rowNumber
            };

            // Parse each column
            policy.BranchCode = GetCellValue(row, columnMap, "Kod", "kod");

            var brans = GetCellValue(row, columnMap, "Brans", "brans");

            // Customer identifier (TC/Tax number)
            policy.CustomerIdentifier = GetCellValue(row, columnMap, "Adr", "adr");

            // Insurance company (Acente)
            policy.InsuranceCompanyCode = GetCellValue(row, columnMap, "Acente", "acente");

            // Policy number
            policy.PolicyNumber = GetCellValue(row, columnMap, "Pol.No", "PolNo", "polno", "policeno");

            // Policy type code (Tec)
            policy.PolicyTypeCode = GetCellValue(row, columnMap, "Tec", "tec");

            // Endorsement number (Z.No)
            var endorsementNo = GetCellValue(row, columnMap, "Z.No", "ZNo", "zno", "zeyilno");
            if (!string.IsNullOrEmpty(endorsementNo) && endorsementNo != "000")
            {
                policy.IsEndorsement = true;
                policy.EndorsementNumber = endorsementNo;
            }

            // Start date (Bas.Tarih)
            policy.StartDate = GetDateValue(row, columnMap, "Bas.Tarih", "BasTarih", "bastarih", "baslangictarihi");

            // End date (Bit.Tarih)
            var endDateCell = GetCellValue(row, columnMap, "Bit.Tarih", "BitTarih", "bittarih", "bitistarihi");
            policy.EndDate = ParseDateFromCell(endDateCell);

            // If end date cell contains policy type code (format: "1/5/2026 601-KN-207044")
            if (!string.IsNullOrEmpty(endDateCell) && endDateCell.Contains(" "))
            {
                var parts = endDateCell.Split(new[] { ' ' }, 2);
                policy.EndDate = ParseDate(parts[0]);

                // The second part might be the policy type code
                if (string.IsNullOrEmpty(policy.PolicyTypeCode))
                {
                    policy.PolicyTypeCode = parts[1].Trim();
                }
            }

            // Customer name
            policy.CustomerName = GetCellValue(row, columnMap, "Sigortalının Adı/Unvan", "sigortalininadiUnvan",
                "sigortaliadi", "musteriad", "musteri");

            // Driver age (Yas)
            var driverAge = GetCellValue(row, columnMap, "Yas", "yas", "surucuyas");
            if (!string.IsNullOrEmpty(driverAge))
            {
                if (int.TryParse(driverAge, out int age))
                {
                    policy.DriverAge = age;
                }
            }

            // Unit (Birim)
            var birim = GetCellValue(row, columnMap, "Birim", "birim");

            // Currency/Rate (Kur)
            var kur = GetCellValue(row, columnMap, "Kur", "kur", "doviz");
            policy.CurrencyCode = ParseCurrencyFromCell(kur, birim);

            // Premium amount (Toplam)
            var premium = GetCellValue(row, columnMap, "Toplam", "toplam", "prim", "primtutar");
            policy.PremiumAmount = ParseDecimal(premium);

            // Commission (Komisyon)
            var commission = GetCellValue(row, columnMap, "Komisyon", "komisyon");
            var commissionDecimal = ParseDecimal(commission);

            // Check if commission is a percentage or amount
            if (commissionDecimal > 0 && commissionDecimal < 100 && policy.PremiumAmount > 0)
            {
                // Likely a percentage
                policy.CommissionRate = commissionDecimal;
                policy.CommissionAmount = policy.PremiumAmount * commissionDecimal / 100;
            }
            else
            {
                // Likely an amount
                policy.CommissionAmount = commissionDecimal;
                if (policy.PremiumAmount > 0)
                {
                    policy.CommissionRate = (commissionDecimal / policy.PremiumAmount) * 100;
                }
            }

            // License plate (plaka)
            policy.PlateNumber = GetCellValue(row, columnMap, "plaka", "plaka", "araçplaka");

            // Vehicle type (Cinsi)
            var vehicleType = GetCellValue(row, columnMap, "Cinsi", "cinsi", "aractipi");

            // Vehicle brand and model (Marka-Model)
            var brandModel = GetCellValue(row, columnMap, "Marka-Model", "MarkaModel", "markamodel", "marka");
            if (!string.IsNullOrEmpty(brandModel))
            {
                // Try to split brand and model
                var parts = brandModel.Split(new[] { ' ' }, 2);
                policy.VehicleBrand = parts[0];
                if (parts.Length > 1)
                {
                    policy.VehicleModel = parts[1];
                }
                else
                {
                    policy.VehicleModel = parts[0];
                }
            }

            // Vehicle year (Yıl)
            var year = GetCellValue(row, columnMap, "Yıl", "Yil", "yil", "model");
            if (!string.IsNullOrEmpty(year))
            {
                if (int.TryParse(year, out int vehicleYear))
                {
                    policy.VehicleYear = vehicleYear;
                }
            }

            // Driver type (Sürücü)
            policy.DriverTypeText = GetCellValue(row, columnMap, "Sürücü", "Surucu", "surucu", "suructipi");

            // Marketer code (Paz.Kod)
            policy.MarketerCode = GetCellValue(row, columnMap, "Paz.Kod", "PazKod", "pazkod", "pazarlamakod");

            // Marketer name (Pazarlama Adı)
            policy.MarketerName = GetCellValue(row, columnMap, "Pazarlama Adı", "PazarlamaAdi", "pazarlamaadi", "pazarlama");

            // Set default status
            policy.Status = PolicyStatus.Active;

            return policy;
        }

        private string? GetCellValue(IXLRow row, Dictionary<string, int> columnMap, params string[] possibleHeaders)
        {
            foreach (var header in possibleHeaders)
            {
                if (columnMap.TryGetValue(header, out int columnIndex))
                {
                    var cell = row.Cell(columnIndex);
                    if (!cell.IsEmpty())
                    {
                        var value = cell.GetString().Trim();
                        if (!string.IsNullOrEmpty(value))
                        {
                            return value;
                        }
                    }
                }
            }

            return null;
        }

        private DateTime? GetDateValue(IXLRow row, Dictionary<string, int> columnMap, params string[] possibleHeaders)
        {
            var cellValue = GetCellValue(row, columnMap, possibleHeaders);
            return ParseDateFromCell(cellValue);
        }

        private DateTime? ParseDateFromCell(string? cellValue)
        {
            if (string.IsNullOrWhiteSpace(cellValue)) return null;

            // Remove any trailing text (like policy codes)
            if (cellValue.Contains(" "))
            {
                cellValue = cellValue.Split(' ')[0];
            }

            return ParseDate(cellValue);
        }

        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;

            // Try various date formats
            string[] formats = new[]
            {
                "d/M/yyyy",
                "dd/MM/yyyy",
                "d.M.yyyy",
                "dd.MM.yyyy",
                "yyyy-MM-dd",
                "M/d/yyyy",
                "MM/dd/yyyy"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString.Trim(), format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime date))
                {
                    return date;
                }
            }

            // Try general parsing as last resort
            if (DateTime.TryParse(dateString, out DateTime generalDate))
            {
                return generalDate;
            }

            return null;
        }

        private decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;

            // Remove any non-numeric characters except , . and -
            var cleanValue = Regex.Replace(value, "[^0-9,.-]", "");

            // Handle Turkish number format (1.234,56) vs English (1,234.56)
            // If there's both comma and period, determine which is decimal separator
            if (cleanValue.Contains(",") && cleanValue.Contains("."))
            {
                var lastComma = cleanValue.LastIndexOf(',');
                var lastPeriod = cleanValue.LastIndexOf('.');

                if (lastComma > lastPeriod)
                {
                    // Turkish format: 1.234,56
                    cleanValue = cleanValue.Replace(".", "").Replace(",", ".");
                }
                else
                {
                    // English format: 1,234.56
                    cleanValue = cleanValue.Replace(",", "");
                }
            }
            else if (cleanValue.Contains(","))
            {
                // Only comma - could be Turkish decimal or English thousands
                // Assume it's a decimal separator if there are 2 digits after it
                var parts = cleanValue.Split(',');
                if (parts.Length == 2 && parts[1].Length <= 2)
                {
                    // Turkish decimal: 1234,56
                    cleanValue = cleanValue.Replace(",", ".");
                }
                else
                {
                    // English thousands: 1,234
                    cleanValue = cleanValue.Replace(",", "");
                }
            }

            if (decimal.TryParse(cleanValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0;
        }

        private string ParseCurrencyFromCell(string? kur, string? birim)
        {
            // Check if currency is explicitly specified
            if (!string.IsNullOrEmpty(kur))
            {
                var normalized = kur.ToUpperInvariant().Trim();

                if (normalized.Contains("TL") || normalized.Contains("TRY"))
                    return "TRY";
                if (normalized.Contains("USD") || normalized.Contains("$"))
                    return "USD";
                if (normalized.Contains("EUR") || normalized.Contains("€"))
                    return "EUR";
                if (normalized.Contains("GBP") || normalized.Contains("£"))
                    return "GBP";
            }

            // Check birim column
            if (!string.IsNullOrEmpty(birim))
            {
                var normalized = birim.ToUpperInvariant().Trim();

                if (normalized.Contains("TL") || normalized.Contains("TRY"))
                    return "TRY";
                if (normalized.Contains("USD") || normalized.Contains("$"))
                    return "USD";
                if (normalized.Contains("EUR") || normalized.Contains("€"))
                    return "EUR";
                if (normalized.Contains("GBP") || normalized.Contains("£"))
                    return "GBP";
            }

            // Default to TRY (Turkish Lira)
            return "TRY";
        }

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
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userId,
                UpdatedDate = DateTime.UtcNow
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
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.UtcNow
                };

                await _unitOfWork.PolicyPayments.AddAsync(payment);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return policy;
        }

        private async Task<string> GenerateEndorsementNumberAsync(int originalPolicyId)
        {
            // Get all endorsements for this policy
            var endorsements = await _unitOfWork.Policies.GetEndorsementsByOriginalPolicyIdAsync(originalPolicyId);

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
                var customer = await _unitOfWork.Customers.GetByTaxOrIdentityNumberAsync(dto.CustomerIdentifier);
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
                    CreatedDate = DateTime.UtcNow,
                    UpdatedBy = userId,
                    UpdatedDate = DateTime.UtcNow
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
                Year = dto.VehicleYear,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = userId,
                UpdatedDate = DateTime.UtcNow
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
                return PaymentMethod.Check;

            return PaymentMethod.Cash;
        }

        private string GeneratePolicyNumber()
        {
            // Generate policy number in format: POL-YYYYMMDD-HHMMSSFFF
            return $"POL-{DateTime.Now:yyyyMMdd-HHmmssfff}";
        }
    }
}
