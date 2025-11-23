using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Calculates claim amounts with deductibles and validates against policy coverage
    /// </summary>
    public class ClaimCalculator : IClaimCalculator
    {
        private readonly ILogger<ClaimCalculator> _logger;
        private const decimal DefaultMaxCoverageMultiplier = 1.5m; // Max claim can be 1.5x premium by default

        public ClaimCalculator(ILogger<ClaimCalculator> logger)
        {
            _logger = logger;
        }

        public decimal CalculatePayableAmount(Policy policy, decimal claimedAmount)
        {
            if (claimedAmount <= 0)
            {
                _logger.LogWarning("Claimed amount must be positive");
                return 0;
            }

            if (!IsPolicyEligibleForClaim(policy, DateTime.UtcNow))
            {
                _logger.LogWarning("Policy {PolicyNumber} is not eligible for claims", policy.PolicyNumber);
                return 0;
            }

            // Apply deductible if specified (for Kasko/Comprehensive policies)
            var deductible = policy.DeductibleAmount ?? 0;

            // Payable amount is claimed amount minus deductible, but never negative
            var payableAmount = Math.Max(0, claimedAmount - deductible);

            // Ensure payable amount doesn't exceed maximum claimable
            var maxClaimable = GetMaximumClaimableAmount(policy);
            if (payableAmount > maxClaimable)
            {
                _logger.LogWarning(
                    "Payable amount {PayableAmount} exceeds maximum claimable {MaxClaimable} for policy {PolicyNumber}",
                    payableAmount, maxClaimable, policy.PolicyNumber);

                payableAmount = maxClaimable;
            }

            _logger.LogInformation(
                "Claim calculation: Claimed={Claimed}, Deductible={Deductible}, Payable={Payable}",
                claimedAmount, deductible, payableAmount);

            return payableAmount;
        }

        public bool ValidateClaimAmount(Policy policy, decimal claimedAmount)
        {
            if (claimedAmount <= 0)
            {
                _logger.LogWarning("Claimed amount must be positive");
                return false;
            }

            if (!IsPolicyEligibleForClaim(policy, DateTime.UtcNow))
            {
                _logger.LogWarning("Policy {PolicyNumber} is not eligible for claims", policy.PolicyNumber);
                return false;
            }

            var maxClaimable = GetMaximumClaimableAmount(policy);
            var deductible = policy.DeductibleAmount ?? 0;
            var potentialPayable = claimedAmount - deductible;

            if (potentialPayable > maxClaimable)
            {
                _logger.LogWarning(
                    "Claimed amount {ClaimedAmount} (minus deductible {Deductible}) exceeds maximum coverage {MaxCoverage}",
                    claimedAmount, deductible, maxClaimable);
                return false;
            }

            return true;
        }

        public decimal GetMaximumClaimableAmount(Policy policy)
        {
            // For vehicle insurance (Kasko), typically the vehicle value or premium-based limit
            // For property insurance, the insured property value
            // For life/health, the coverage amount specified
            // As a default, we'll use a multiplier of the premium amount

            // For Kasko policies with driver accident coverage, add that to max claimable
            if (policy.HasDriverAccidentCoverage == true && policy.DriverAccidentCoverageAmount.HasValue)
            {
                return policy.PremiumAmount * DefaultMaxCoverageMultiplier + policy.DriverAccidentCoverageAmount.Value;
            }

            // Default: Maximum claimable is a multiple of the premium
            return policy.PremiumAmount * DefaultMaxCoverageMultiplier;
        }

        public bool IsPolicyEligibleForClaim(Policy policy, DateTime claimDate)
        {
            // Policy must be active
            if (policy.Status != PolicyStatus.Active)
            {
                _logger.LogWarning("Policy {PolicyNumber} is not active (Status: {Status})",
                    policy.PolicyNumber, policy.Status);
                return false;
            }

            // Claim date must be within policy coverage period
            if (claimDate < policy.StartDate || claimDate > policy.EndDate)
            {
                _logger.LogWarning(
                    "Claim date {ClaimDate} is outside policy coverage period ({StartDate} to {EndDate})",
                    claimDate, policy.StartDate, policy.EndDate);
                return false;
            }

            return true;
        }
    }
}
