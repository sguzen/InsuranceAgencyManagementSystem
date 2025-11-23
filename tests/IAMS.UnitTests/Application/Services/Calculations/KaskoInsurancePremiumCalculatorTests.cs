using FluentAssertions;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Services.Calculations
{
    public class KaskoInsurancePremiumCalculatorTests
    {
        private readonly Mock<IVehicleRepository> _mockVehicleRepository;
        private readonly Mock<ILogger<KaskoInsurancePremiumCalculator>> _mockLogger;
        private readonly KaskoInsurancePremiumCalculator _calculator;

        public KaskoInsurancePremiumCalculatorTests()
        {
            _mockVehicleRepository = new Mock<IVehicleRepository>();
            _mockLogger = new Mock<ILogger<KaskoInsurancePremiumCalculator>>();
            _calculator = new KaskoInsurancePremiumCalculator(
                _mockVehicleRepository.Object,
                _mockLogger.Object);
        }

        private Policy CreateValidPolicy(int? vehicleId = 1, decimal? existingPremium = null)
        {
            return new Policy
            {
                Id = 1,
                PolicyNumber = "KAS-001",
                CustomerId = 1,
                InsuranceCompanyId = 1,
                PolicyTypeId = 2,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                VehicleId = vehicleId,
                PremiumAmount = existingPremium ?? 0m,
                Status = PolicyStatus.Active,
                CurrencyId = 1
            };
        }

        private Vehicle CreateVehicle(decimal? currentValue = 100000m, int? modelYear = null)
        {
            return new Vehicle
            {
                Id = 1,
                PlateNumber = "34ABC123",
                ChassisNumber = "CHASSIS123",
                BrandId = 1,
                ModelId = 1,
                ModelYear = modelYear ?? DateTime.Now.Year - 2,
                CurrentValue = currentValue,
                VehicleType = VehicleType.Passenger,
                FuelType = VehicleFuelType.Gasoline,
                UsageType = VehicleUsageType.Private,
                CustomerId = 1,
                CurrencyId = 1
            };
        }

        [Fact]
        public void PolicyTypeCode_ShouldReturnKAS()
        {
            // Assert
            _calculator.PolicyTypeCode.Should().Be("KAS");
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithExistingPremium_ShouldApplyDiscountsAndCoverages()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 5000m);
            policy.NoClaimDiscountRate = 10m;
            policy.HasGlassCoverage = true; // +5%

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Base: 5000
            // After no-claim (10%): 4500
            // After glass coverage (+5% of 5000): 4750
            premium.Should().Be(4750m);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithVehicle_ShouldCalculateBasedOnValue()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(currentValue: 200000m, modelYear: DateTime.Now.Year - 2);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Vehicle value 200k, age 2 years (rate 3%)
            premium.Should().Be(6000m); // 200000 * 0.03
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithOlderVehicle_ShouldUseHigherRate()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(currentValue: 100000m, modelYear: DateTime.Now.Year - 8);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Vehicle value 100k, age 8 years (6-10 range, rate 4%)
            premium.Should().Be(4000m); // 100000 * 0.04
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithVeryOldVehicle_ShouldUseHighestRate()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(currentValue: 80000m, modelYear: DateTime.Now.Year - 12);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Vehicle value 80k, age 12 years (>10 years, rate 5%)
            premium.Should().Be(4000m); // 80000 * 0.05
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithGlassCoverage_ShouldAdd5Percent()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 4000m);
            policy.HasGlassCoverage = true;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(4200m); // 4000 + (4000 * 0.05)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithTheftCoverage_ShouldAdd8Percent()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 5000m);
            policy.HasTheftCoverage = true;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(5400m); // 5000 + (5000 * 0.08)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithNaturalDisasterCoverage_ShouldAdd6Percent()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 3000m);
            policy.HasNaturalDisasterCoverage = true;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(3180m); // 3000 + (3000 * 0.06)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithDriverAccidentCoverage_ShouldAddBasedOnAmount()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 4000m);
            policy.HasDriverAccidentCoverage = true;
            policy.DriverAccidentCoverageAmount = 100000;

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(4100m); // 4000 + (100000 * 0.001)
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithAllCoverages_ShouldAddAllPremiums()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 10000m);
            policy.HasGlassCoverage = true;              // +5% = 500
            policy.HasTheftCoverage = true;              // +8% = 800
            policy.HasNaturalDisasterCoverage = true;    // +6% = 600
            policy.HasDriverAccidentCoverage = true;
            policy.DriverAccidentCoverageAmount = 50000; // +50

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Base: 10000
            // Glass: +500, Theft: +800, Disaster: +600, Driver: +50
            premium.Should().Be(11950m);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithDiscountsAndCoverages_ShouldApplyCorrectly()
        {
            // Arrange
            var policy = CreateValidPolicy(existingPremium: 10000m);
            policy.NoClaimDiscountRate = 20m;           // -20% = 8000
            policy.FleetDiscountRate = 10m;             // -10% of original = 7000
            policy.HasGlassCoverage = true;             // +5% of discounted 7000 = +350
            policy.HasTheftCoverage = true;             // +8% of discounted 7000 = +560

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            // Base: 10000
            // After discounts: 7000
            // After coverages (% of discounted): 7000 + 350 + 560 = 7910
            premium.Should().Be(7910m);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WithoutVehicle_ShouldUseDefaultPremium()
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: null);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(2000m); // Default minimum kasko premium
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
        public void ValidateForCalculation_WithNegativeDeductible_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.DeductibleAmount = -500m;

            // Act
            var isValid = _calculator.ValidateForCalculation(policy);

            // Assert
            isValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(100000, 2, 3000)]   // 100k vehicle, 2 years old, 3% rate
        [InlineData(200000, 8, 8000)]   // 200k vehicle, 8 years old, 4% rate
        [InlineData(150000, 12, 7500)]  // 150k vehicle, 12 years old, 5% rate
        public async Task CalculatePremiumAsync_WithVariousVehicles_ShouldCalculateCorrectly(
            decimal vehicleValue, int vehicleAge, decimal expectedPremium)
        {
            // Arrange
            var policy = CreateValidPolicy(vehicleId: 1);
            var vehicle = CreateVehicle(
                currentValue: vehicleValue,
                modelYear: DateTime.Now.Year - vehicleAge);

            _mockVehicleRepository
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(vehicle);

            // Act
            var premium = await _calculator.CalculatePremiumAsync(policy);

            // Assert
            premium.Should().Be(expectedPremium);
        }
    }
}
