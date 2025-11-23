using IAMS.Application.Configuration;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Routes premium calculations to either internal or external service based on configuration
    /// </summary>
    public class RoutingPremiumCalculator : IPolicyPremiumCalculator
    {
        private readonly IPolicyPremiumCalculator _internalCalculator;
        private readonly IExternalPremiumCalculationService _externalService;
        private readonly PremiumCalculationSettings _settings;
        private readonly ILogger<RoutingPremiumCalculator> _logger;

        public string PolicyTypeCode => _internalCalculator.PolicyTypeCode;

        public RoutingPremiumCalculator(
            IPolicyPremiumCalculator internalCalculator,
            IExternalPremiumCalculationService externalService,
            IOptions<PremiumCalculationSettings> settings,
            ILogger<RoutingPremiumCalculator> logger)
        {
            _internalCalculator = internalCalculator;
            _externalService = externalService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<decimal> CalculatePremiumAsync(Policy policy)
        {
            // Check if this policy type should use external calculation
            var shouldUseExternal = ShouldUseExternalCalculation();

            if (!shouldUseExternal)
            {
                _logger.LogDebug(
                    "Using internal calculator for policy type '{PolicyTypeCode}'",
                    PolicyTypeCode);
                return await _internalCalculator.CalculatePremiumAsync(policy);
            }

            try
            {
                _logger.LogInformation(
                    "Attempting external calculation for policy type '{PolicyTypeCode}'",
                    PolicyTypeCode);

                var premium = await _externalService.CalculatePremiumAsync(policy, PolicyTypeCode);

                _logger.LogInformation(
                    "Successfully calculated premium using external service: {Premium}",
                    premium);

                return premium;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "External calculation failed for policy type '{PolicyTypeCode}'",
                    PolicyTypeCode);

                if (_settings.FallbackToInternalOnFailure)
                {
                    _logger.LogWarning(
                        "Falling back to internal calculator for policy type '{PolicyTypeCode}'",
                        PolicyTypeCode);

                    return await _internalCalculator.CalculatePremiumAsync(policy);
                }

                throw;
            }
        }

        public decimal ApplyDiscounts(decimal basePremium, Policy policy)
        {
            // Discounts are always applied using internal logic
            return _internalCalculator.ApplyDiscounts(basePremium, policy);
        }

        public bool ValidateForCalculation(Policy policy)
        {
            // Validation uses internal logic
            return _internalCalculator.ValidateForCalculation(policy);
        }

        private bool ShouldUseExternalCalculation()
        {
            if (!_settings.ExternalService.Enabled)
            {
                return false;
            }

            // Check if this policy type is configured for external calculation
            return _settings.ExternalCalculationPolicyTypes.Contains(
                PolicyTypeCode,
                StringComparer.OrdinalIgnoreCase);
        }
    }
}
