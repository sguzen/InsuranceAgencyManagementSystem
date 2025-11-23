using FluentAssertions;
using IAMS.Application.Configuration;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace IAMS.UnitTests.Application.Services.Calculations
{
    public class RoutingPremiumCalculatorTests
    {
        private readonly Mock<IPolicyPremiumCalculator> _mockInternalCalculator;
        private readonly Mock<IExternalPremiumCalculationService> _mockExternalService;
        private readonly Mock<ILogger<RoutingPremiumCalculator>> _mockLogger;
        private readonly PremiumCalculationSettings _settings;

        public RoutingPremiumCalculatorTests()
        {
            _mockInternalCalculator = new Mock<IPolicyPremiumCalculator>();
            _mockExternalService = new Mock<IExternalPremiumCalculationService>();
            _mockLogger = new Mock<ILogger<RoutingPremiumCalculator>>();

            _settings = new PremiumCalculationSettings
            {
                ExternalCalculationPolicyTypes = new List<string> { "TRF" },
                FallbackToInternalOnFailure = true,
                ExternalService = new ExternalServiceSettings
                {
                    Enabled = true,
                    BaseUrl = "https://test.example.com",
                    TimeoutSeconds = 30
                }
            };

            _mockInternalCalculator.Setup(x => x.PolicyTypeCode).Returns("TRF");
        }

        private RoutingPremiumCalculator CreateCalculator()
        {
            return new RoutingPremiumCalculator(
                _mockInternalCalculator.Object,
                _mockExternalService.Object,
                Options.Create(_settings),
                _mockLogger.Object);
        }

        [Fact]
        public void PolicyTypeCode_ShouldReturnInternalCalculatorCode()
        {
            // Arrange
            var calculator = CreateCalculator();

            // Act
            var code = calculator.PolicyTypeCode;

            // Assert
            code.Should().Be("TRF");
        }

        [Fact]
        public async Task CalculatePremiumAsync_WhenExternalServiceDisabled_ShouldUseInternalCalculator()
        {
            // Arrange
            _settings.ExternalService.Enabled = false;
            var calculator = CreateCalculator();
            var policy = new Policy { Id = 1, PolicyTypeId = 1 };
            _mockInternalCalculator.Setup(x => x.CalculatePremiumAsync(policy))
                .ReturnsAsync(1000m);

            // Act
            var result = await calculator.CalculatePremiumAsync(policy);

            // Assert
            result.Should().Be(1000m);
            _mockInternalCalculator.Verify(x => x.CalculatePremiumAsync(policy), Times.Once);
            _mockExternalService.Verify(
                x => x.CalculatePremiumAsync(It.IsAny<Policy>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WhenPolicyTypeNotInExternalList_ShouldUseInternalCalculator()
        {
            // Arrange
            _settings.ExternalCalculationPolicyTypes = new List<string> { "KAS" }; // Not TRF
            var calculator = CreateCalculator();
            var policy = new Policy { Id = 1, PolicyTypeId = 1 };
            _mockInternalCalculator.Setup(x => x.CalculatePremiumAsync(policy))
                .ReturnsAsync(1500m);

            // Act
            var result = await calculator.CalculatePremiumAsync(policy);

            // Assert
            result.Should().Be(1500m);
            _mockInternalCalculator.Verify(x => x.CalculatePremiumAsync(policy), Times.Once);
            _mockExternalService.Verify(
                x => x.CalculatePremiumAsync(It.IsAny<Policy>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WhenExternalServiceEnabledAndPolicyTypeMatches_ShouldUseExternalService()
        {
            // Arrange
            var calculator = CreateCalculator();
            var policy = new Policy { Id = 1, PolicyTypeId = 1 };
            _mockExternalService.Setup(x => x.CalculatePremiumAsync(policy, "TRF"))
                .ReturnsAsync(2000m);

            // Act
            var result = await calculator.CalculatePremiumAsync(policy);

            // Assert
            result.Should().Be(2000m);
            _mockExternalService.Verify(x => x.CalculatePremiumAsync(policy, "TRF"), Times.Once);
            _mockInternalCalculator.Verify(
                x => x.CalculatePremiumAsync(It.IsAny<Policy>()),
                Times.Never);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WhenExternalServiceFails_AndFallbackEnabled_ShouldUseInternalCalculator()
        {
            // Arrange
            var calculator = CreateCalculator();
            var policy = new Policy { Id = 1, PolicyTypeId = 1 };
            _mockExternalService.Setup(x => x.CalculatePremiumAsync(policy, "TRF"))
                .ThrowsAsync(new InvalidOperationException("External service unavailable"));
            _mockInternalCalculator.Setup(x => x.CalculatePremiumAsync(policy))
                .ReturnsAsync(1200m);

            // Act
            var result = await calculator.CalculatePremiumAsync(policy);

            // Assert
            result.Should().Be(1200m);
            _mockExternalService.Verify(x => x.CalculatePremiumAsync(policy, "TRF"), Times.Once);
            _mockInternalCalculator.Verify(x => x.CalculatePremiumAsync(policy), Times.Once);
        }

        [Fact]
        public async Task CalculatePremiumAsync_WhenExternalServiceFails_AndFallbackDisabled_ShouldThrow()
        {
            // Arrange
            _settings.FallbackToInternalOnFailure = false;
            var calculator = CreateCalculator();
            var policy = new Policy { Id = 1, PolicyTypeId = 1 };
            var expectedException = new InvalidOperationException("External service unavailable");
            _mockExternalService.Setup(x => x.CalculatePremiumAsync(policy, "TRF"))
                .ThrowsAsync(expectedException);

            // Act
            Func<Task> act = async () => await calculator.CalculatePremiumAsync(policy);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("External service unavailable");
            _mockInternalCalculator.Verify(
                x => x.CalculatePremiumAsync(It.IsAny<Policy>()),
                Times.Never);
        }

        [Fact]
        public void ApplyDiscounts_ShouldDelegateToInternalCalculator()
        {
            // Arrange
            var calculator = CreateCalculator();
            var policy = new Policy { NoClaimDiscountRate = 10m };
            _mockInternalCalculator.Setup(x => x.ApplyDiscounts(1000m, policy))
                .Returns(900m);

            // Act
            var result = calculator.ApplyDiscounts(1000m, policy);

            // Assert
            result.Should().Be(900m);
            _mockInternalCalculator.Verify(x => x.ApplyDiscounts(1000m, policy), Times.Once);
        }

        [Fact]
        public void ValidateForCalculation_ShouldDelegateToInternalCalculator()
        {
            // Arrange
            var calculator = CreateCalculator();
            var policy = new Policy { Id = 1 };
            _mockInternalCalculator.Setup(x => x.ValidateForCalculation(policy))
                .Returns(true);

            // Act
            var result = calculator.ValidateForCalculation(policy);

            // Assert
            result.Should().BeTrue();
            _mockInternalCalculator.Verify(x => x.ValidateForCalculation(policy), Times.Once);
        }
    }
}
