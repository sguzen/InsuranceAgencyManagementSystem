// IAMS.Application/Services/CustomerCodeGenerator.cs
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services
{
    public interface ICustomerCodeGenerator
    {
        Task<string> GenerateAsync();
        Task<bool> IsCodeUniqueAsync(string code);
        string GenerateFallbackCode();
    }

    public class CustomerCodeGenerator : ICustomerCodeGenerator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<CustomerCodeGenerator> _logger;

        // Configuration - could be moved to appsettings.json
        private const string CODE_PREFIX = "MUS";
        private const int SEQUENCE_LENGTH = 6;
        private const int MAX_RETRY_ATTEMPTS = 5;

        public CustomerCodeGenerator(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<CustomerCodeGenerator> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<string> GenerateAsync()
        {
            try
            {
                // Get tenant-specific prefix if configured
                var tenantPrefix = _currentTenantService.GetSetting("CustomerCodePrefix", CODE_PREFIX);
                var year = DateTime.Now.Year.ToString();

                // Try sequence-based generation first
                var sequenceCode = await GenerateSequenceBasedCodeAsync(tenantPrefix, year);
                if (!string.IsNullOrEmpty(sequenceCode))
                {
                    return sequenceCode;
                }

                // Fallback to timestamp-based if sequence fails
                _logger.LogWarning("Sequence-based code generation failed for tenant {TenantId}, using timestamp fallback");
                return GenerateTimestampBasedCode(tenantPrefix);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating customer code for tenant {TenantId}");
                return GenerateFallbackCode();
            }
        }

        private async Task<string> GenerateSequenceBasedCodeAsync(string prefix, string year)
        {
            for (int attempt = 0; attempt < MAX_RETRY_ATTEMPTS; attempt++)
            {
                try
                {
                    // Get the highest sequence number for this tenant and year
                    var lastCustomer = await GetLastCustomerForYearAsync(year);
                    var nextSequence = ExtractSequenceFromCode(lastCustomer?.CustomerCode, prefix, year) + 1;

                    // Generate the code
                    var code = $"{prefix}{year}{nextSequence:D6}";

                    // Verify uniqueness (double-check for concurrency)
                    if (await IsCodeUniqueAsync(code))
                    {
                        _logger.LogDebug("Generated customer code {Code} for tenant {TenantId} (attempt {Attempt})",
                            code, attempt + 1);
                        return code;
                    }

                    _logger.LogWarning("Generated code {Code} already exists for tenant {TenantId}, retrying...",
                        code);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt} failed for sequence-based code generation", attempt + 1);
                }
            }

            return string.Empty; // All attempts failed
        }

        private async Task<Customer?> GetLastCustomerForYearAsync(string year)
        {
            try
            {
                // Get customers from this year with codes matching the pattern
                var yearStart = new DateTime(int.Parse(year), 1, 1);
                var yearEnd = yearStart.AddYears(1).AddDays(-1);

                var customers = await _unitOfWork.Customers.GetCustomersCreatedBetweenAsync(
                    yearStart, yearEnd);

                // Filter and sort by sequence number
                return customers
                    .Where(c => !string.IsNullOrEmpty(c.CustomerCode) &&
                               c.CustomerCode.Contains(year))
                    .OrderByDescending(c => ExtractSequenceFromCode(c.CustomerCode, CODE_PREFIX, year))
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting last customer for year {Year}", year);
                return null;
            }
        }

        private static int ExtractSequenceFromCode(string? customerCode, string prefix, string year)
        {
            if (string.IsNullOrEmpty(customerCode))
                return 0;

            try
            {
                var expectedPrefix = $"{prefix}{year}";
                if (!customerCode.StartsWith(expectedPrefix))
                    return 0;

                var sequencePart = customerCode.Substring(expectedPrefix.Length);
                return int.TryParse(sequencePart, out var sequence) ? sequence : 0;
            }
            catch
            {
                return 0;
            }
        }

        private string GenerateTimestampBasedCode(string prefix)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var randomSuffix = new Random().Next(10, 99); // Add randomness to avoid collisions
            return $"{prefix}{timestamp}{randomSuffix}";
        }

        public string GenerateFallbackCode()
        {
            var guid = Guid.NewGuid().ToString("N")[..8].ToUpper();
            return $"{CODE_PREFIX}FALLBACK{guid}";
        }

        public async Task<bool> IsCodeUniqueAsync(string code)
        {
            try
            {
                var existing = await _unitOfWork.Customers.GetByCustomerCodeAsync(code);
                return existing == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking code uniqueness for {Code}", code);
                return false; // Assume not unique on error to be safe
            }
        }
    }
}
