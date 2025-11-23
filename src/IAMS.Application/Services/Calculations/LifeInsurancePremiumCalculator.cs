using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Premium calculator for Life Insurance (HAY - Hayat, FK - Ferdi Kaza)
    /// </summary>
    public class LifeInsurancePremiumCalculator : BasePolicyPremiumCalculator
    {
        public LifeInsurancePremiumCalculator(
            ILogger<LifeInsurancePremiumCalculator> logger) : base(logger)
        {
        }

        public override string PolicyTypeCode => "HAY"; // Also handles FK

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

            // Life insurance premiums are based on:
            // - Age of insured
            // - Health status
            // - Coverage amount
            // - Term length
            // - Smoking status
            // Since we don't have detailed insured person info in current model,
            // we'll use a simplified calculation

            decimal basePremium = CalculateBasePremium(policy);

            // Apply discounts (rare in life insurance, but might exist for non-smokers, etc.)
            var finalPremium = ApplyDiscounts(basePremium, policy);

            _logger.LogInformation(
                "Life insurance premium calculated: Base={Base}, Final={Final}",
                basePremium, finalPremium);

            return await Task.FromResult(finalPremium);
        }

        private decimal CalculateBasePremium(Policy policy)
        {
            // Default base premium for life insurance
            decimal basePremium = 800m;

            // Life insurance can be long-term (up to 20 years)
            var termMonths = GetTermLengthInMonths(policy);
            var years = termMonths / 12m;

            // For long-term policies, annual premium might be lower
            // but total premium over the term is higher
            if (years > 10)
            {
                // Long-term life insurance
                basePremium = basePremium * 0.8m; // 20% discount on annual premium for long-term commitment
            }
            else if (years > 5)
            {
                basePremium = basePremium * 0.9m; // 10% discount
            }

            // For personal accident insurance (FK), premiums are typically lower
            // This would be determined by PolicyType, but we're simplifying here

            _logger.LogInformation(
                "Calculated base premium for life insurance: {Premium} (Term: {Years} years)",
                basePremium, years);

            return basePremium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            if (!ValidateBasicPolicyData(policy))
                return false;

            // Life insurance can have very long terms (up to 240 months = 20 years)
            var termMonths = GetTermLengthInMonths(policy);
            if (termMonths > 240)
            {
                _logger.LogWarning("Life insurance term exceeds maximum of 20 years");
            }

            if (policy.PremiumAmount <= 0)
            {
                _logger.LogWarning("Life insurance should have a premium amount set");
            }

            return true;
        }
    }
}
