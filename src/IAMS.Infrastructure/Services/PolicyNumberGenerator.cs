using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IAMS.Infrastructure.Services
{
    public class PolicyNumberGenerator : IPolicyNumberGenerator
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PolicyNumberGenerator> _logger;

        public PolicyNumberGenerator(IUnitOfWork unitOfWork, ILogger<PolicyNumberGenerator> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<string> GenerateAsync(int tenantId, int insuranceCompanyId, int policyTypeId)
        {
            try
            {
                // Get insurance company for prefix
                var insuranceCompany = await _unitOfWork.InsuranceCompanies.GetByIdAsync(insuranceCompanyId);
                var prefix = insuranceCompany?.Code ?? "POL";

                // Get current year (last 2 digits)
                var year = DateTime.Now.Year.ToString().Substring(2);

                // Get next sequence number
                var sequence = await GetNextSequenceAsync(tenantId, insuranceCompanyId);

                // Format: PREFIX-YY-XXXXXX (e.g., AXA-24-000001)
                var policyNumber = $"{prefix}-{year}-{sequence:D6}";

                _logger.LogInformation("Generated policy number: {PolicyNumber}", policyNumber);
                return policyNumber;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating policy number");
                throw;
            }
        }

        public async Task<string> GenerateRenewalNumberAsync(string originalPolicyNumber)
        {
            var renewalSuffix = "-R";
            var basePolicyNumber = originalPolicyNumber;

            // Check if it's already a renewal
            if (originalPolicyNumber.Contains("-R"))
            {
                var parts = originalPolicyNumber.Split("-R");
                basePolicyNumber = parts[0];

                if (parts.Length > 1 && int.TryParse(parts[1], out int renewalNumber))
                {
                    renewalSuffix = $"-R{renewalNumber + 1}";
                }
                else
                {
                    renewalSuffix = "-R2";
                }
            }

            return basePolicyNumber + renewalSuffix;
        }

        public bool IsValidFormat(string policyNumber, int insuranceCompanyId)
        {
            if (string.IsNullOrWhiteSpace(policyNumber))
                return false;

            var parts = policyNumber.Split('-');
            if (parts.Length < 3)
                return false;

            // Check year part (2 digits)
            if (parts[1].Length != 2 || !int.TryParse(parts[1], out _))
                return false;

            // Check sequence part (6 digits, may have renewal suffix)
            var sequencePart = parts[2].Split("-R")[0];
            if (sequencePart.Length != 6 || !int.TryParse(sequencePart, out _))
                return false;

            return true;
        }

        private async Task<int> GetNextSequenceAsync(int tenantId, int insuranceCompanyId)
        {
            try
            {
                // This is a simplified implementation - you may need to add this method to your repository
                // For now, generate a random sequence between 1-999999
                var random = new Random();
                return random.Next(1, 1000000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next sequence");
                return 1;
            }
        }
    }
}