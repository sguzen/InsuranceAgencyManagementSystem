using IAMS.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Calculates commissions based on policy type and insurance company rates from database
    /// </summary>
    public class CommissionCalculator : ICommissionCalculator
    {
        private readonly ICommissionRateRepository _commissionRateRepository;
        private readonly ILogger<CommissionCalculator> _logger;
        private const decimal DefaultCommissionRate = 10.0m; // Default 10% if no rate found

        public CommissionCalculator(
            ICommissionRateRepository commissionRateRepository,
            ILogger<CommissionCalculator> logger)
        {
            _commissionRateRepository = commissionRateRepository;
            _logger = logger;
        }

        public async Task<(decimal CommissionAmount, decimal CommissionRate)> CalculateCommissionAsync(
            int policyTypeId,
            int insuranceCompanyId,
            decimal premiumAmount)
        {
            if (premiumAmount <= 0)
            {
                _logger.LogWarning("Cannot calculate commission for zero or negative premium amount");
                return (0, 0);
            }

            // Lookup commission rate from database
            var commissionRate = await GetCommissionRateAsync(policyTypeId, insuranceCompanyId);

            // Use default rate if not found
            if (!commissionRate.HasValue)
            {
                _logger.LogWarning(
                    "No commission rate found for PolicyType {PolicyTypeId} and Company {CompanyId}. Using default rate {DefaultRate}%",
                    policyTypeId, insuranceCompanyId, DefaultCommissionRate);

                commissionRate = DefaultCommissionRate;
            }

            var commissionAmount = premiumAmount * (commissionRate.Value / 100);

            _logger.LogInformation(
                "Calculated commission: Premium={Premium}, Rate={Rate}%, Amount={Amount}",
                premiumAmount, commissionRate.Value, commissionAmount);

            return (commissionAmount, commissionRate.Value);
        }

        public async Task<decimal?> GetCommissionRateAsync(int policyTypeId, int insuranceCompanyId)
        {
            try
            {
                var commissionRateEntity = await _commissionRateRepository
                    .GetByPolicyTypeAndCompanyAsync(policyTypeId, insuranceCompanyId);

                if (commissionRateEntity == null)
                {
                    return null;
                }

                return commissionRateEntity.Rate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving commission rate for PolicyType {PolicyTypeId} and Company {CompanyId}",
                    policyTypeId, insuranceCompanyId);
                return null;
            }
        }
    }
}
