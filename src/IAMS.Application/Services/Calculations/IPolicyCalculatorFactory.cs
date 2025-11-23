using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Factory interface for resolving the appropriate premium calculator based on policy type
    /// </summary>
    public interface IPolicyCalculatorFactory
    {
        /// <summary>
        /// Gets the appropriate premium calculator for a policy type code
        /// </summary>
        /// <param name="policyTypeCode">The policy type code (e.g., "TRF", "KAS", "HAY")</param>
        /// <returns>The appropriate premium calculator</returns>
        IPolicyPremiumCalculator GetCalculator(string policyTypeCode);

        /// <summary>
        /// Gets the appropriate premium calculator for a policy
        /// </summary>
        /// <param name="policy">The policy</param>
        /// <returns>The appropriate premium calculator</returns>
        Task<IPolicyPremiumCalculator> GetCalculatorForPolicyAsync(Policy policy);
    }
}
