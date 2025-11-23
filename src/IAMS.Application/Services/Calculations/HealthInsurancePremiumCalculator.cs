using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Premium calculator for Health Insurance (TSS - Tamamlayıcı Sağlık, OSS - Özel Sağlık, SEY - Seyahat)
    /// </summary>
    public class HealthInsurancePremiumCalculator : BasePolicyPremiumCalculator
    {
        public HealthInsurancePremiumCalculator(
            ILogger<HealthInsurancePremiumCalculator> logger) : base(logger)
        {
        }

        public override string PolicyTypeCode => "TSS"; // Also handles OSS, SEY

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

            // Health insurance premiums are typically based on:
            // - Age of insured
            // - Health conditions
            // - Coverage limits
            // - Family vs individual
            // Since we don't have detailed insured person info in current model,
            // we'll use a simplified calculation

            decimal basePremium = CalculateBasePremium(policy);

            // Apply discounts (less common in health insurance, but possible for family plans)
            var finalPremium = ApplyDiscounts(basePremium, policy);

            _logger.LogInformation(
                "Health insurance premium calculated: Base={Base}, Final={Final}",
                basePremium, finalPremium);

            return await Task.FromResult(finalPremium);
        }

        private decimal CalculateBasePremium(Policy policy)
        {
            // Default base premium for health insurance
            decimal basePremium = 1500m;

            // Adjust based on coverage period
            var termMonths = GetTermLengthInMonths(policy);

            if (termMonths < 12)
            {
                // Pro-rata for shorter terms (e.g., travel insurance)
                basePremium = basePremium * (termMonths / 12m);

                // Travel insurance is typically cheaper
                if (termMonths <= 3)
                {
                    basePremium = Math.Max(100m, basePremium * 0.3m);
                }
            }

            _logger.LogInformation("Calculated base premium for health insurance: {Premium}", basePremium);

            return basePremium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            if (!ValidateBasicPolicyData(policy))
                return false;

            if (policy.PremiumAmount <= 0)
            {
                _logger.LogWarning("Health insurance should have a premium amount set");
            }

            return true;
        }
    }
}
