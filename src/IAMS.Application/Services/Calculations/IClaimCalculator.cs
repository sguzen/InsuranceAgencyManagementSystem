using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Interface for calculating claim amounts with deductibles and coverage validation
    /// </summary>
    public interface IClaimCalculator
    {
        /// <summary>
        /// Calculates the payable claim amount after applying deductibles
        /// </summary>
        /// <param name="policy">The policy being claimed against</param>
        /// <param name="claimedAmount">The amount being claimed</param>
        /// <returns>The actual payable amount after deductibles</returns>
        decimal CalculatePayableAmount(Policy policy, decimal claimedAmount);

        /// <summary>
        /// Validates if a claim amount is within policy coverage limits
        /// </summary>
        /// <param name="policy">The policy being claimed against</param>
        /// <param name="claimedAmount">The amount being claimed</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateClaimAmount(Policy policy, decimal claimedAmount);

        /// <summary>
        /// Gets the maximum claimable amount for a policy
        /// </summary>
        /// <param name="policy">The policy</param>
        /// <returns>The maximum claimable amount</returns>
        decimal GetMaximumClaimableAmount(Policy policy);

        /// <summary>
        /// Checks if the policy is eligible for claims based on its status and dates
        /// </summary>
        /// <param name="policy">The policy to check</param>
        /// <param name="claimDate">The date of the claim</param>
        /// <returns>True if eligible, false otherwise</returns>
        bool IsPolicyEligibleForClaim(Policy policy, DateTime claimDate);
    }
}
