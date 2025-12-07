using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Premium calculator for Comprehensive Insurance (KAS, MINKAS - Kasko Sigortası)
    /// </summary>
    public class KaskoInsurancePremiumCalculator : BasePolicyPremiumCalculator
    {
        private readonly IVehicleRepository _vehicleRepository;

        public KaskoInsurancePremiumCalculator(
            IVehicleRepository vehicleRepository,
            ILogger<KaskoInsurancePremiumCalculator> logger) : base(logger)
        {
            _vehicleRepository = vehicleRepository;
        }

        public override string PolicyTypeCode => "KAS"; // Also handles MINKAS

        public override async Task<decimal> CalculatePremiumAsync(Policy policy)
        {
            if (!ValidateForCalculation(policy))
            {
                throw new InvalidOperationException("Policy data is invalid for premium calculation");
            }

            // If premium is already set (e.g., from external source), use it as base
            if (policy.PremiumAmount > 0)
            {
                _logger.LogInformation("Using existing premium amount {Amount} as base", policy.PremiumAmount);
                var finalPremium = ApplyDiscounts(policy.PremiumAmount, policy);
                return AddOptionalCoverages(finalPremium, policy);
            }

            // Calculate base premium based on vehicle value
            decimal basePremium = 0;

            if (policy.VehicleId.HasValue)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(policy.VehicleId.Value);
                if (vehicle != null)
                {
                    basePremium = CalculateBasePremiumForVehicle(vehicle);
                }
            }

            if (basePremium == 0)
            {
                basePremium = 2000m; // Default minimum kasko premium
                _logger.LogWarning("Using default base premium {Amount} for kasko insurance", basePremium);
            }

            // Apply discounts
            var discountedPremium = ApplyDiscounts(basePremium, policy);

            // Add optional coverage premiums
            var totalPremium = AddOptionalCoverages(discountedPremium, policy);

            _logger.LogInformation(
                "Kasko insurance premium calculated: Base={Base}, Discounted={Discounted}, Final={Final}",
                basePremium, discountedPremium, totalPremium);

            return totalPremium;
        }

        private decimal CalculateBasePremiumForVehicle(Vehicle vehicle)
        {
            // Kasko premium is typically a percentage of vehicle market value
            // This is a simplified calculation

            decimal vehicleValue = vehicle.CurrentValue ?? 50000m; // Default value if not set
            decimal premiumRate = 0.03m; // 3% of vehicle value (simplified)

            // Adjust rate based on vehicle age
            var vehicleAge = DateTime.Now.Year - (vehicle.ModelYear ?? DateTime.Now.Year);
            if (vehicleAge > 10)
                premiumRate = 0.05m; // Older vehicles have higher risk/rate
            else if (vehicleAge > 5)
                premiumRate = 0.04m;

            decimal basePremium = vehicleValue * premiumRate;

            _logger.LogInformation(
                "Calculated kasko base premium for vehicle {Plate}: Value={Value}, Rate={Rate}%, Premium={Premium}",
                vehicle.PlateNumber, vehicleValue, premiumRate * 100, basePremium);

            return basePremium;
        }

        private decimal AddOptionalCoverages(decimal basePremium, Policy policy)
        {
            decimal additionalPremium = 0;

            // Add glass coverage premium
            if (policy.HasGlassCoverage == true)
            {
                var glassCoverage = basePremium * 0.05m; // 5% additional
                additionalPremium += glassCoverage;
                _logger.LogInformation("Added glass coverage premium: {Amount}", glassCoverage);
            }

            // Add theft coverage premium
            if (policy.HasTheftCoverage == true)
            {
                var theftCoverage = basePremium * 0.08m; // 8% additional
                additionalPremium += theftCoverage;
                _logger.LogInformation("Added theft coverage premium: {Amount}", theftCoverage);
            }

            // Add natural disaster coverage premium
            if (policy.HasNaturalDisasterCoverage == true)
            {
                var disasterCoverage = basePremium * 0.06m; // 6% additional
                additionalPremium += disasterCoverage;
                _logger.LogInformation("Added natural disaster coverage premium: {Amount}", disasterCoverage);
            }

            // Add driver accident coverage premium
            if (policy.HasDriverAccidentCoverage == true && policy.DriverAccidentCoverageAmount.HasValue)
            {
                // Driver accident coverage based on coverage amount
                var driverAccidentPremium = policy.DriverAccidentCoverageAmount.Value * 0.001m; // 0.1% of coverage
                additionalPremium += driverAccidentPremium;
                _logger.LogInformation("Added driver accident coverage premium: {Amount}", driverAccidentPremium);
            }

            return basePremium + additionalPremium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            if (!ValidateBasicPolicyData(policy))
                return false;

            // Kasko should have vehicle information
            if (!policy.VehicleId.HasValue && policy.PremiumAmount <= 0)
            {
                _logger.LogWarning("Kasko policy should have vehicle information or existing premium amount");
            }

            // Validate deductible amount if set
            if (policy.DeductibleAmount.HasValue && policy.DeductibleAmount.Value < 0)
            {
                _logger.LogError("Deductible amount cannot be negative");
                return false;
            }

            return true;
        }
    }
}
