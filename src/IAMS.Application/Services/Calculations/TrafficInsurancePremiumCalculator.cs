using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Calculations
{
    /// <summary>
    /// Premium calculator for Traffic Insurance (TRF - Trafik Sigortası)
    /// </summary>
    public class TrafficInsurancePremiumCalculator : BasePolicyPremiumCalculator
    {
        private readonly IVehicleRepository _vehicleRepository;

        public TrafficInsurancePremiumCalculator(
            IVehicleRepository vehicleRepository,
            ILogger<TrafficInsurancePremiumCalculator> logger) : base(logger)
        {
            _vehicleRepository = vehicleRepository;
        }

        public override string PolicyTypeCode => "TRF";

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
                return ApplyDiscounts(policy.PremiumAmount, policy);
            }

            // Calculate base premium based on vehicle information
            decimal basePremium = 0;

            if (policy.VehicleId.HasValue)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(policy.VehicleId.Value);
                if (vehicle != null)
                {
                    // Simple calculation based on vehicle age and type
                    // In real scenario, this would involve complex rating tables
                    basePremium = CalculateBasePremiumForVehicle(vehicle);
                }
            }

            if (basePremium == 0)
            {
                // Fallback: use minimum premium or a default amount
                basePremium = 500m; // Default minimum traffic insurance premium
                _logger.LogWarning("Using default base premium {Amount} for traffic insurance", basePremium);
            }

            // Apply payment frequency factor (if paying monthly vs annually)
            basePremium = AdjustForPaymentFrequency(basePremium, policy);

            // Apply discounts
            var finalPremium = ApplyDiscounts(basePremium, policy);

            _logger.LogInformation(
                "Traffic insurance premium calculated: Base={Base}, Final={Final}",
                basePremium, finalPremium);

            return finalPremium;
        }

        private decimal CalculateBasePremiumForVehicle(Vehicle vehicle)
        {
            // Base premium calculation factors:
            // 1. Vehicle age
            // 2. Engine power/displacement
            // 3. Vehicle type
            // This is a simplified example - real calculations are much more complex

            decimal basePremium = 500m; // Minimum base

            // Factor in vehicle age (newer vehicles may have higher premiums)
            var vehicleAge = DateTime.Now.Year - (vehicle.ModelYear ?? DateTime.Now.Year);
            if (vehicleAge <= 1)
                basePremium += 200m;
            else if (vehicleAge <= 5)
                basePremium += 100m;

            // Factor in engine power if available
            if (vehicle.EnginePower.HasValue)
            {
                basePremium += vehicle.EnginePower.Value * 0.5m; // 0.5 TL per HP
            }

            _logger.LogInformation(
                "Calculated base premium for vehicle {Plate}: {Premium}",
                vehicle.PlateNumber, basePremium);

            return basePremium;
        }

        private decimal AdjustForPaymentFrequency(decimal basePremium, Policy policy)
        {
            // If paying in installments, there might be an additional fee
            // For annual payment, might get a small discount
            // This is simplified - actual implementation would depend on business rules

            var termMonths = GetTermLengthInMonths(policy);

            if (termMonths < 12)
            {
                // Short-term policy, calculate pro-rata
                return basePremium * (termMonths / 12m);
            }

            return basePremium;
        }

        public override bool ValidateForCalculation(Policy policy)
        {
            if (!ValidateBasicPolicyData(policy))
                return false;

            // Traffic insurance should have vehicle information
            if (!policy.VehicleId.HasValue && policy.PremiumAmount <= 0)
            {
                _logger.LogWarning("Traffic insurance policy should have vehicle information or existing premium amount");
                // Not a hard failure - we can use default premium
            }

            return true;
        }
    }
}
