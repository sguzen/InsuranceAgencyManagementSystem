using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Premium calculator for Property Insurance (KNT - Konut, DASK - Deprem, ISY - İşyeri)
    /// </summary>
    public class PropertyInsurancePremiumCalculator : BasePolicyPremiumCalculator
    {
        public PropertyInsurancePremiumCalculator(
            ILogger<PropertyInsurancePremiumCalculator> logger) : base(logger)
        {
        }

        public override string PolicyTypeCode => "KNT"; // Also handles DASK, ISY

        public override async Task<decimal> CalculatePremiumAsync(Policy policy)
        {
            if (!ValidateForCalculation(policy))
            {
                throw new InvalidOperationException("Policy data is invalid for premium calculation");
            }

            // If premium is already set (e.g., from external source or manual entry), use it
            if (policy.PremiumAmount > 0)
            {
                _logger.LogInformation("Using existing premium amount {Amount} as base", policy.PremiumAmount);
                return ApplyDiscounts(policy.PremiumAmount, policy);
            }

            // For property insurance, premium is typically calculated based on:
            // - Property value
            // - Location (risk zone)
            // - Construction type
            // - Age of building
            // Since we don't have property entity in the current model,
            // we'll use a simplified approach

            decimal basePremium = CalculateBasePremium(policy);

            // Apply discounts
            var finalPremium = ApplyDiscounts(basePremium, policy);

            _logger.LogInformation(
                "Property insurance premium calculated: Base={Base}, Final={Final}",
                basePremium, finalPremium);

            return await Task.FromResult(finalPremium);
        }

        private decimal CalculateBasePremium(Policy policy)
        {
            // Default base premiums by type
            // In a real system, this would be much more sophisticated
            decimal basePremium = 500m;

            // Adjust based on coverage period
            var termMonths = GetTermLengthInMonths(policy);
            if (termMonths < 12)
            {
                // Pro-rata for shorter terms
                basePremium = basePremium * (termMonths / 12m);
            }
            else if (termMonths > 12)
            {
                // Multi-year policies might get a small discount
                var years = termMonths / 12m;
                basePremium = basePremium * years * 0.95m; // 5% discount for multi-year
            }

            _logger.LogInformation("Calculated base premium for property insurance: {Premium}", basePremium);

            return basePremium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            if (!ValidateBasicPolicyData(policy))
                return false;

            // Property insurance specific validations
            // In a real system, we'd validate property information
            // For now, we'll allow calculation with just the premium amount

            if (policy.PremiumAmount <= 0)
            {
                _logger.LogWarning("Property insurance should have a premium amount set");
            }

            return true;
        }
    }
}
