using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Interface for calculating policy premiums with type-specific logic
    /// </summary>
    public interface IPolicyPremiumCalculator
    {
        /// <summary>
        /// Gets the policy type code this calculator handles (e.g., "TRF", "KAS", "HAY")
        /// </summary>
        string PolicyTypeCode { get; }

        /// <summary>
        /// Calculates the total premium amount for a policy
        /// </summary>
        /// <param name="policy">The policy to calculate premium for</param>
        /// <returns>The calculated premium amount</returns>
        Task<decimal> CalculatePremiumAsync(Policy policy);

        /// <summary>
        /// Applies discounts to the base premium
        /// </summary>
        /// <param name="basePremium">The base premium amount before discounts</param>
        /// <param name="policy">The policy with discount information</param>
        /// <returns>The premium after applying discounts</returns>
        decimal ApplyDiscounts(decimal basePremium, Policy policy);

        /// <summary>
        /// Validates if the policy data is complete for premium calculation
        /// </summary>
        /// <param name="policy">The policy to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateForCalculation(Policy policy);
    }
}
