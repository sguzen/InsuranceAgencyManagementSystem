using IAMS.Application.Configuration;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Factory for resolving the appropriate premium calculator based on policy type
    /// </summary>
    public class PolicyCalculatorFactory : IPolicyCalculatorFactory
    {
        private readonly IEnumerable<IPolicyPremiumCalculator> _calculators;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly IExternalPremiumCalculationService _externalService;
        private readonly PremiumCalculationSettings _settings;
        private readonly ILogger<PolicyCalculatorFactory> _logger;
        private readonly ILogger<RoutingPremiumCalculator> _routingLogger;

        // Mapping of policy type codes to calculator types
        private readonly Dictionary<string, Type> _calculatorMapping = new()
        {
            // Vehicle Insurance
            { "TRF", typeof(TrafficInsurancePremiumCalculator) },
            { "KAS", typeof(KaskoInsurancePremiumCalculator) },
            { "MINKAS", typeof(KaskoInsurancePremiumCalculator) },

            // Property Insurance
            { "KNT", typeof(PropertyInsurancePremiumCalculator) },
            { "DASK", typeof(PropertyInsurancePremiumCalculator) },
            { "ISY", typeof(PropertyInsurancePremiumCalculator) },

            // Health Insurance
            { "TSS", typeof(HealthInsurancePremiumCalculator) },
            { "OSS", typeof(HealthInsurancePremiumCalculator) },
            { "SEY", typeof(HealthInsurancePremiumCalculator) },

            // Life Insurance
            { "HAY", typeof(LifeInsurancePremiumCalculator) },
            { "FK", typeof(LifeInsurancePremiumCalculator) },

            // Liability Insurance
            { "MMS", typeof(LiabilityInsurancePremiumCalculator) },
            { "USS", typeof(LiabilityInsurancePremiumCalculator) }
        };

        public PolicyCalculatorFactory(
            IEnumerable<IPolicyPremiumCalculator> calculators,
            IPolicyTypeRepository policyTypeRepository,
            IExternalPremiumCalculationService externalService,
            IOptions<PremiumCalculationSettings> settings,
            ILogger<PolicyCalculatorFactory> logger,
            ILogger<RoutingPremiumCalculator> routingLogger)
        {
            _calculators = calculators;
            _policyTypeRepository = policyTypeRepository;
            _externalService = externalService;
            _settings = settings.Value;
            _logger = logger;
            _routingLogger = routingLogger;
        }

        public IPolicyPremiumCalculator GetCalculator(string policyTypeCode)
        {
            if (string.IsNullOrWhiteSpace(policyTypeCode))
            {
                _logger.LogError("Policy type code cannot be null or empty");
                throw new ArgumentException("Policy type code is required", nameof(policyTypeCode));
            }

            // Find the calculator type for this policy type code
            if (!_calculatorMapping.TryGetValue(policyTypeCode, out var calculatorType))
            {
                _logger.LogWarning(
                    "No specific calculator found for policy type '{PolicyTypeCode}'. Using default calculator.",
                    policyTypeCode);

                // Default to property insurance calculator as a fallback
                calculatorType = typeof(PropertyInsurancePremiumCalculator);
            }

            // Find the calculator instance of the appropriate type
            var calculator = _calculators.FirstOrDefault(c => c.GetType() == calculatorType);

            if (calculator == null)
            {
                _logger.LogError(
                    "Calculator of type {CalculatorType} not found in DI container",
                    calculatorType.Name);
                throw new InvalidOperationException(
                    $"Calculator for policy type '{policyTypeCode}' is not registered");
            }

            _logger.LogInformation(
                "Resolved calculator {CalculatorType} for policy type '{PolicyTypeCode}'",
                calculator.GetType().Name, policyTypeCode);

            // Wrap with routing calculator if needed
            return WrapWithRoutingIfNeeded(calculator);
        }

        /// <summary>
        /// Wraps a calculator with routing logic if external calculation is configured
        /// </summary>
        private IPolicyPremiumCalculator WrapWithRoutingIfNeeded(IPolicyPremiumCalculator calculator)
        {
            // If external service is not enabled, return calculator as-is
            if (!_settings.ExternalService.Enabled)
            {
                return calculator;
            }

            // Check if this policy type should use external calculation
            var shouldUseRouting = _settings.ExternalCalculationPolicyTypes.Contains(
                calculator.PolicyTypeCode,
                StringComparer.OrdinalIgnoreCase);

            if (!shouldUseRouting)
            {
                return calculator;
            }

            _logger.LogInformation(
                "Wrapping calculator {CalculatorType} with routing for policy type '{PolicyTypeCode}'",
                calculator.GetType().Name, calculator.PolicyTypeCode);

            // Wrap with routing calculator
            return new RoutingPremiumCalculator(
                calculator,
                _externalService,
                Options.Create(_settings),
                _routingLogger);
        }

        public async Task<IPolicyPremiumCalculator> GetCalculatorForPolicyAsync(Policy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            // Get the policy type from repository
            var policyType = await _policyTypeRepository.GetByIdAsync(policy.PolicyTypeId);

            if (policyType == null)
            {
                _logger.LogError("Policy type with ID {PolicyTypeId} not found", policy.PolicyTypeId);
                throw new InvalidOperationException($"Policy type with ID {policy.PolicyTypeId} not found");
            }

            return GetCalculator(policyType.Code);
        }
    }
}
