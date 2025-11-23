using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Base abstract class for policy premium calculations with common discount logic
    /// </summary>
    public abstract class BasePolicyPremiumCalculator : IPolicyPremiumCalculator
    {
        protected readonly ILogger _logger;

        protected BasePolicyPremiumCalculator(ILogger logger)
        {
            _logger = logger;
        }

        public abstract string PolicyTypeCode { get; }

        public abstract Task<decimal> CalculatePremiumAsync(Policy policy);

        public virtual decimal ApplyDiscounts(decimal basePremium, Policy policy)
        {
            var discountedPremium = basePremium;

            // Apply no-claim discount if applicable
            if (policy.NoClaimDiscountRate.HasValue && policy.NoClaimDiscountRate.Value > 0)
            {
                var noClaimDiscount = basePremium * (policy.NoClaimDiscountRate.Value / 100);
                discountedPremium -= noClaimDiscount;

                _logger.LogInformation(
                    "Applied no-claim discount: {Rate}% ({Years} years), Amount: {Discount}",
                    policy.NoClaimDiscountRate.Value,
                    policy.NoClaimYears ?? 0,
                    noClaimDiscount);
            }

            // Apply fleet discount if applicable
            if (policy.FleetDiscountRate.HasValue && policy.FleetDiscountRate.Value > 0)
            {
                var fleetDiscount = basePremium * (policy.FleetDiscountRate.Value / 100);
                discountedPremium -= fleetDiscount;

                _logger.LogInformation(
                    "Applied fleet discount: {Rate}%, Amount: {Discount}",
                    policy.FleetDiscountRate.Value,
                    fleetDiscount);
            }

            // Ensure premium doesn't go below zero
            discountedPremium = Math.Max(0, discountedPremium);

            if (discountedPremium != basePremium)
            {
                _logger.LogInformation(
                    "Total discounts applied: Base={Base}, Final={Final}, Savings={Savings}",
                    basePremium, discountedPremium, basePremium - discountedPremium);
            }

            return discountedPremium;
        }

        public abstract bool ValidateForCalculation(Policy policy);

        /// <summary>
        /// Helper method to calculate term length in months
        /// </summary>
        protected int GetTermLengthInMonths(Policy policy)
        {
            var startDate = policy.StartDate;
            var endDate = policy.EndDate;

            var months = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;

            // Add 1 if the end day is greater than or equal to start day
            if (endDate.Day >= startDate.Day)
                months++;

            return Math.Max(1, months);
        }

        /// <summary>
        /// Helper to validate basic policy data
        /// </summary>
        protected bool ValidateBasicPolicyData(Policy policy)
        {
            if (policy == null)
            {
                _logger.LogError("Policy cannot be null");
                return false;
            }

            if (policy.StartDate >= policy.EndDate)
            {
                _logger.LogError("Policy end date must be after start date");
                return false;
            }

            if (policy.PolicyTypeId <= 0)
            {
                _logger.LogError("Invalid PolicyTypeId");
                return false;
            }

            if (policy.InsuranceCompanyId <= 0)
            {
                _logger.LogError("Invalid InsuranceCompanyId");
                return false;
            }

            return true;
        }
    }
}
