using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Interface for calculating commissions based on policy type and insurance company
    /// </summary>
    public interface ICommissionCalculator
    {
        /// <summary>
        /// Calculates commission amount and rate for a policy
        /// </summary>
        /// <param name="policyTypeId">The policy type ID</param>
        /// <param name="insuranceCompanyId">The insurance company ID</param>
        /// <param name="premiumAmount">The premium amount</param>
        /// <returns>Tuple of (CommissionAmount, CommissionRate)</returns>
        Task<(decimal CommissionAmount, decimal CommissionRate)> CalculateCommissionAsync(
            int policyTypeId,
            int insuranceCompanyId,
            decimal premiumAmount);

        /// <summary>
        /// Gets the commission rate for a specific policy type and company
        /// </summary>
        /// <param name="policyTypeId">The policy type ID</param>
        /// <param name="insuranceCompanyId">The insurance company ID</param>
        /// <returns>The commission rate percentage</returns>
        Task<decimal?> GetCommissionRateAsync(int policyTypeId, int insuranceCompanyId);
    }
}
