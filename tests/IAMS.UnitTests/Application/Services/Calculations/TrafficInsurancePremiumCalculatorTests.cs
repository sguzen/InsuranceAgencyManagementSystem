using FluentAssertions;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Shared.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Services.Calculations
{
    public class TrafficInsurancePremiumCalculatorTests
    {
        private readonly Mock<IVehicleRepository> _mockVehicleRepository;
        private readonly Mock<ILogger<TrafficInsurancePremiumCalculator>> _mockLogger;
        private readonly TrafficInsurancePremiumCalculator _calculator;

        public TrafficInsurancePremiumCalculatorTests()
        {
            _mockVehicleRepository = new Mock<IVehicleRepository>();
            _mockLogger = new Mock<ILogger<TrafficInsurancePremiumCalculator>>();
            _calculator = new TrafficInsurancePremiumCalculator(
                _mockVehicleRepository.Object,
                _mockLogger.Object);
        }

        private Policy CreateValidPolicy(int? vehicleId = 1, decimal? existingPremium = null)
        {
            return new Policy
            {
                Id = 1,
                PolicyNumber = "TRF-001",
                CustomerId = 1,
                InsuranceCompanyId = 1,
                PolicyTypeId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                VehicleId = vehicleId,
                PremiumAmount = existingPremium ?? 0m,
                Status = PolicyStatus.Active,
                CurrencyId = 1
            };
        }

        private Vehicle CreateVehicle(int? modelYear = null, int? enginePower = null)
        {
            return new Vehicle
            {
                Id = 1,
                PlateNumber = "34ABC123",
                ChassisNumber = "CHASSIS123",
                BrandId = 1,
                ModelId = 1,
                ModelYear = modelYear ?? DateTime.Now.Year - 2,
                EnginePower = enginePower ?? 100,
                VehicleType = VehicleType.Car,
                FuelType = VehicleFuelType.Gasoline,
                UsageType = VehicleUsageType.Private,
                CustomerId = 1,
                CurrencyId = 1
            };
        }

        [Fact]
        public void PolicyTypeCode_ShouldReturnTRF()
        {
            // Assert
            _calculator.PolicyTypeCode.Should().Be("TRF");
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithExistingPremium_ShouldApplyDiscounts()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 1000m);
            policy.NoClaimDiscountRate = 10m; // 10% discount

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(900m); // 1000 - (1000 * 0.10)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithVehicle_ShouldCalculateBasedOnVehicleData()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(modelYear: DateTime.Now.Year - 1, enginePower: 150);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().BeGreaterThan(0);
            // New vehicle (1 year old) gets higher base
            // Plus 150 HP * 0.5 = 75 additional
            premium.Should().BeGreaterThan(700m); // 500 + 200 (new) + 75 (HP)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithoutVehicle_ShouldUseDefaultPremium()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: null);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(500m); // Default minimum
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithNoClaimDiscount_ShouldApplyDiscount()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 1000m);
            policy.NoClaimDiscountRate = 15m;
            policy.NoClaimYears = 3;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(850m); // 1000 - (1000 * 0.15)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithFleetDiscount_ShouldApplyDiscount()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 2000m);
            policy.FleetDiscountRate = 20m;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(1600m); // 2000 - (2000 * 0.20)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithBothDiscounts_ShouldApplyBoth()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 1000m);
            policy.NoClaimDiscountRate = 10m;
            policy.FleetDiscountRate = 10m;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Base: 1000
            // After no-claim (10%): 900
            // After fleet (10% of original 1000): 800
            premium.Should().Be(800m);
        }

        [Fact]
        public void ValidateForCalculation_WithValidPolicy_ShouldReturnTrue()
        {
            // Arrange
            var policy = CreateValidPolicy();

            // Act
            var isValid = _calculator.ValidateForCalculation(policy);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateForCalculation_WithNullPolicy_ShouldReturnFalse()
        {
            // Act
            var isValid = _calculator.ValidateForCalculation(null!);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateForCalculation_WithInvalidDates_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.EndDate = policy.StartDate.AddDays(-1); // End before start

            // Act
            var isValid = _calculator.ValidateForCalculation(policy);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithOldVehicle_ShouldCalculateCorrectly()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(modelYear: DateTime.Now.Year - 8, enginePower: 120);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().BeGreaterThan(0);
            // Older vehicle (>5 years) gets no age bonus, just base + HP
            premium.Should().Be(560m); // 500 (base) + 0 (no age bonus) + 60 (HP: 120 * 0.5)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithVeryOldVehicle_ShouldNotAddAgeBonus()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(modelYear: DateTime.Now.Year - 15, enginePower: 80);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().BeGreaterThan(0);
            // Very old vehicle gets no age bonus
            premium.Should().Be(540m); // 500 (base) + 0 (no age bonus) + 40 (HP: 80 * 0.5)
        }
    }
}
