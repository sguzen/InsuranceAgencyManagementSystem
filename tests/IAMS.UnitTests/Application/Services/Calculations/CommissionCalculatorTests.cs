using FluentAssertions;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Services.Calculations
{
    public class CommissionCalculatorTests
    {
        private readonly Mock<ICommissionRateRepository> _mockCommissionRateRepository;
        private readonly Mock<ILogger<CommissionCalculator>> _mockLogger;
        private readonly CommissionCalculator _calculator;

        public CommissionCalculatorTests()
        {
            _mockCommissionRateRepository = new Mock<ICommissionRateRepository>();
            _mockLogger = new Mock<ILogger<CommissionCalculator>>();
            _calculator = new CommissionCalculator(
                _mockCommissionRateRepository.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task CalculateCommissionAsync_WithConfiguredRate_ShouldReturnCorrectCommission()
        {
            // Arrange
            int policyTypeId = 1;
            int insuranceCompanyId = 2;
            decimal premiumAmount = 1000m;
            decimal expectedRate = 15m;

            var commissionRateEntity = new CommissionRate
            {
                Id = 1,
                PolicyTypeId = policyTypeId,
                InsuranceCompanyId = insuranceCompanyId,
                Rate = expectedRate,
                IsActive = true
            };

            _mockCommissionRateRepository
                .Setup(x => x.GetByPolicyTypeAndCompanyAsync(policyTypeId, insuranceCompanyId))
                .ReturnsAsync(commissionRateEntity);

            // Act
            var (commissionAmount, commissionRate) = await _calculator.CalculateCommissionAsync(
                policyTypeId, insuranceCompanyId, premiumAmount);

            // Assert
            commissionRate.Should().Be(expectedRate);
            commissionAmount.Should().Be(150m); // 1000 * 0.15
        }

        [Fact]
        public async Task CalculateCommissionAsync_WithNoConfiguredRate_ShouldUseDefaultRate()
        {
            // Arrange
            int policyTypeId = 1;
            int insuranceCompanyId = 2;
            decimal premiumAmount = 2000m;
            decimal defaultRate = 10m; // Default is 10%

            _mockCommissionRateRepository
                .Setup(x => x.GetByPolicyTypeAndCompanyAsync(policyTypeId, insuranceCompanyId))
                .ReturnsAsync((CommissionRate?)null);

            // Act
            var (commissionAmount, commissionRate) = await _calculator.CalculateCommissionAsync(
                policyTypeId, insuranceCompanyId, premiumAmount);

            // Assert
            commissionRate.Should().Be(defaultRate);
            commissionAmount.Should().Be(200m); // 2000 * 0.10
        }

        [Fact]
        public async Task CalculateCommissionAsync_WithZeroPremium_ShouldReturnZeroCommission()
        {
            // Arrange
            int policyTypeId = 1;
            int insuranceCompanyId = 2;
            decimal premiumAmount = 0m;

            // Act
            var (commissionAmount, commissionRate) = await _calculator.CalculateCommissionAsync(
                policyTypeId, insuranceCompanyId, premiumAmount);

            // Assert
            commissionAmount.Should().Be(0m);
            commissionRate.Should().Be(0m);
        }

        [Fact]
        public async Task CalculateCommissionAsync_WithNegativePremium_ShouldReturnZeroCommission()
        {
            // Arrange
            int policyTypeId = 1;
            int insuranceCompanyId = 2;
            decimal premiumAmount = -1000m;

            // Act
            var (commissionAmount, commissionRate) = await _calculator.CalculateCommissionAsync(
                policyTypeId, insuranceCompanyId, premiumAmount);

            // Assert
            commissionAmount.Should().Be(0m);
            commissionRate.Should().Be(0m);
        }

        [Fact]
        public async Task GetCommissionRateAsync_WithExistingRate_ShouldReturnRate()
        {
            // Arrange
            int policyTypeId = 1;
            int insuranceCompanyId = 2;
            decimal expectedRate = 12.5m;

            var commissionRateEntity = new CommissionRate
            {
                Id = 1,
                PolicyTypeId = policyTypeId,
                InsuranceCompanyId = insuranceCompanyId,
                Rate = expectedRate,
                IsActive = true
            };

            _mockCommissionRateRepository
                .Setup(x => x.GetByPolicyTypeAndCompanyAsync(policyTypeId, insuranceCompanyId))
                .ReturnsAsync(commissionRateEntity);

            // Act
            var rate = await _calculator.GetCommissionRateAsync(policyTypeId, insuranceCompanyId);

            // Assert
            rate.Should().Be(expectedRate);
        }

        [Fact]
        public async Task GetCommissionRateAsync_WithNoRate_ShouldReturnNull()
        {
            // Arrange
            int policyTypeId = 1;
            int insuranceCompanyId = 2;

            _mockCommissionRateRepository
                .Setup(x => x.GetByPolicyTypeAndCompanyAsync(policyTypeId, insuranceCompanyId))
                .ReturnsAsync((CommissionRate?)null);

            // Act
            var rate = await _calculator.GetCommissionRateAsync(policyTypeId, insuranceCompanyId);

            // Assert
            rate.Should().BeNull();
        }

        [Theory]
        [InlineData(1000, 10, 100)]
        [InlineData(5000, 15, 750)]
        [InlineData(2500, 12.5, 312.5)]
        [InlineData(10000, 8, 800)]
        public async Task CalculateCommissionAsync_WithVariousPremiums_ShouldCalculateCorrectly(
            decimal premium, decimal rate, decimal expectedCommission)
        {
            // Arrange
            var commissionRateEntity = new CommissionRate
            {
                Id = 1,
                PolicyTypeId = 1,
                InsuranceCompanyId = 1,
                Rate = rate,
                IsActive = true
            };

            _mockCommissionRateRepository
                .Setup(x => x.GetByPolicyTypeAndCompanyAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(commissionRateEntity);

            // Act
            var (commissionAmount, commissionRate) = await _calculator.CalculateCommissionAsync(
                1, 1, premium);

            // Assert
            commissionAmount.Should().Be(expectedCommission);
            commissionRate.Should().Be(rate);
        }
    }
}
