using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Premium calculator for Liability Insurance (MMS - Mesleki Mali Sorumluluk, USS - Üçüncü Şahıs)
    /// </summary>
    public class LiabilityInsurancePremiumCalculator : BasePolicyPremiumCalculator
    {
        public LiabilityInsurancePremiumCalculator(
            ILogger<LiabilityInsurancePremiumCalculator> logger) : base(logger)
        {
        }

        public override string PolicyTypeCode => "MMS"; // Also handles USS

        public override async Task<decimal> CalculatePremiumAsync(Policy policy)
        {
            if (!ValidateForCalculation(policy))
            {
                throw new InvalidOperationException("Policy data is invalid for premium calculation");
            }

            // If premium is already set, use it
            if (policy.PremiumAmount > 0)
            {
                _logger.LogInformation("Using existing premium amount {Amount} as base", policy.PremiumAmount);
                return ApplyDiscounts(policy.PremiumAmount, policy);
            }

            // Liability insurance premiums are based on:
            // - Type of business/profession
            // - Coverage limits
            // - Claims history
            // - Revenue/turnover

            decimal basePremium = CalculateBasePremium(policy);

            // Apply discounts
            var finalPremium = ApplyDiscounts(basePremium, policy);

            _logger.LogInformation(
                "Liability insurance premium calculated: Base={Base}, Final={Final}",
                basePremium, finalPremium);

            return await Task.FromResult(finalPremium);
        }

        private decimal CalculateBasePremium(Policy policy)
        {
            // Default base premium for liability insurance
            decimal basePremium = 1000m;

            // Adjust based on coverage period
            var termMonths = GetTermLengthInMonths(policy);
            if (termMonths < 12)
            {
                // Pro-rata for shorter terms
                basePremium = basePremium * (termMonths / 12m);
            }

            _logger.LogInformation("Calculated base premium for liability insurance: {Premium}", basePremium);

            return basePremium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            if (!ValidateBasicPolicyData(policy))
                return false;

            if (policy.PremiumAmount <= 0)
            {
                _logger.LogWarning("Liability insurance should have a premium amount set");
            }

            return true;
        }
    }
}
