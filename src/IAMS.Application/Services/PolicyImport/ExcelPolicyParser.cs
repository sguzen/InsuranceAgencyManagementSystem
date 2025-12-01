using IAMS.Application.DTOs.Policy;
using IAMS.Application.Interfaces.Services;
using IAMS.Domain.Enums;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IAMS.Application.Services.PolicyImport
{
    /// <summary>
    /// Parses Excel files to extract policy data
    /// </summary>
    public class ExcelPolicyParser : IExcelPolicyParser
    {
        private readonly ILogger<ExcelPolicyParser> _logger;

        public ExcelPolicyParser(ILogger<ExcelPolicyParser> logger)
        {
            _logger = logger;
        }

        public async Task<List<ImportPolicyDto>> ParseFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            // Create a temporary file from the stream
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var fileStream = File.Create(tempFile))
                {
                    await stream.CopyToAsync(fileStream, cancellationToken);
                }

                return await ParseFromFileAsync(tempFile, cancellationToken);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public Task<List<ImportPolicyDto>> ParseFromFileAsync(string filePath, CancellationToken cancellationToken = default)
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
            if (!string.IsNullOrEmpty(driverAge) && int.TryParse(driverAge, out int age))
            {
                policy.DriverAge = age;
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
            if (!string.IsNullOrEmpty(year) && int.TryParse(year, out int vehicleYear))
            {
                policy.VehicleYear = vehicleYear;
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
    }
}
