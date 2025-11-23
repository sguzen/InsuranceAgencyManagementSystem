using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Factory for resolving the appropriate premium calculator based on policy type
    /// </summary>
    public class PolicyCalculatorFactory : IPolicyCalculatorFactory
    {
        private readonly IEnumerable<IPolicyPremiumCalculator> _calculators;
        private readonly IPolicyTypeRepository _policyTypeRepository;
        private readonly ILogger<PolicyCalculatorFactory> _logger;

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
            ILogger<PolicyCalculatorFactory> logger)
        {
            _calculators = calculators;
            _policyTypeRepository = policyTypeRepository;
            _logger = logger;
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

            return calculator;
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
