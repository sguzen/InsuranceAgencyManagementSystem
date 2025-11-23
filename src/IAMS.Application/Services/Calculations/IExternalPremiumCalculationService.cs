using IAMS.Domain.Entities;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Interface for external premium calculation services
    /// </summary>
    public interface IExternalPremiumCalculationService
    {
        /// <summary>
        /// Calculates premium using an external service
        /// </summary>
        /// <param name="policy">The policy to calculate premium for</param>
        /// <param name="policyTypeCode">The policy type code (e.g., "TRF", "KAS")</param>
        /// <returns>The calculated premium amount</returns>
        Task<decimal> CalculatePremiumAsync(Policy policy, string policyTypeCode);

        /// <summary>
        /// Checks if the external service is available
        /// </summary>
        /// <returns>True if service is available, false otherwise</returns>
        Task<bool> IsAvailableAsync();
    }
}
