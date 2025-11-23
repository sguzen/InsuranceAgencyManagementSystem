using FluentAssertions;
using IAMS.Application.Services.Calculations;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace IAMS.UnitTests.Application.Services.Calculations
{
    public class ClaimCalculatorTests
    {
        private readonly Mock<ILogger<ClaimCalculator>> _mockLogger;
        private readonly ClaimCalculator _calculator;

        public ClaimCalculatorTests()
        {
            _mockLogger = new Mock<ILogger<ClaimCalculator>>();
            _calculator = new ClaimCalculator(_mockLogger.Object);
        }

        private Policy CreateValidPolicy(decimal premiumAmount = 1000m, decimal? deductible = null)
        {
            return new Policy
            {
                Id = 1,
                PolicyNumber = "POL-001",
                CustomerId = 1,
                InsuranceCompanyId = 1,
                PolicyTypeId = 1,
                StartDate = DateTime.Today.AddMonths(-6),
                EndDate = DateTime.Today.AddMonths(6),
                PremiumAmount = premiumAmount,
                DeductibleAmount = deductible,
                Status = PolicyStatus.Active,
                CurrencyId = 1
            };
        }

        [Fact]
        public void CalculatePayableAmount_WithNoDeductible_ShouldReturnFullClaimedAmount()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 2000m, deductible: null);
            decimal claimedAmount = 1500m;

            // Act
            var payableAmount = _calculator.CalculatePayableAmount(policy, claimedAmount);

            // Assert
            payableAmount.Should().Be(1500m);
        }

        [Fact]
        public void CalculatePayableAmount_WithDeductible_ShouldSubtractDeductible()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 2000m, deductible: 500m);
            decimal claimedAmount = 1500m;

            // Act
            var payableAmount = _calculator.CalculatePayableAmount(policy, claimedAmount);

            // Assert
            payableAmount.Should().Be(1000m); // 1500 - 500
        }

        [Fact]
        public void CalculatePayableAmount_WhenDeductibleExceedsClaimedAmount_ShouldReturnZero()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 2000m, deductible: 1000m);
            decimal claimedAmount = 500m;

            // Act
            var payableAmount = _calculator.CalculatePayableAmount(policy, claimedAmount);

            // Assert
            payableAmount.Should().Be(0m);
        }

        [Fact]
        public void CalculatePayableAmount_WhenExceedsMaxCoverage_ShouldCapAtMaximum()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 1000m, deductible: 0m);
            decimal claimedAmount = 5000m; // More than 1.5x premium (1500)

            // Act
            var payableAmount = _calculator.CalculatePayableAmount(policy, claimedAmount);

            // Assert
            payableAmount.Should().Be(1500m); // Premium * 1.5
        }

        [Fact]
        public void ValidateClaimAmount_WithValidAmount_ShouldReturnTrue()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 2000m, deductible: 500m);
            decimal claimedAmount = 2000m;

            // Act
            var isValid = _calculator.ValidateClaimAmount(policy, claimedAmount);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void ValidateClaimAmount_WithExcessiveAmount_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 1000m, deductible: 200m);
            decimal claimedAmount = 10000m; // Much more than max coverage

            // Act
            var isValid = _calculator.ValidateClaimAmount(policy, claimedAmount);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void ValidateClaimAmount_WithZeroOrNegativeAmount_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();

            // Act
            var isValidZero = _calculator.ValidateClaimAmount(policy, 0m);
            var isValidNegative = _calculator.ValidateClaimAmount(policy, -100m);

            // Assert
            isValidZero.Should().BeFalse();
            isValidNegative.Should().BeFalse();
        }

        [Fact]
        public void GetMaximumClaimableAmount_WithBasicPolicy_ShouldReturn1Point5TimesPremium()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 2000m);

            // Act
            var maxClaimable = _calculator.GetMaximumClaimableAmount(policy);

            // Assert
            maxClaimable.Should().Be(3000m); // 2000 * 1.5
        }

        [Fact]
        public void GetMaximumClaimableAmount_WithDriverAccidentCoverage_ShouldIncludeCoverageAmount()
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: 2000m);
            policy.HasDriverAccidentCoverage = true;
            policy.DriverAccidentCoverageAmount = 50000;

            // Act
            var maxClaimable = _calculator.GetMaximumClaimableAmount(policy);

            // Assert
            maxClaimable.Should().Be(53000m); // (2000 * 1.5) + 50000
        }

        [Fact]
        public void IsPolicyEligibleForClaim_WithActivePolicy_ShouldReturnTrue()
        {
            // Arrange
            var policy = CreateValidPolicy();
            var claimDate = DateTime.Today;

            // Act
            var isEligible = _calculator.IsPolicyEligibleForClaim(policy, claimDate);

            // Assert
            isEligible.Should().BeTrue();
        }

        [Fact]
        public void IsPolicyEligibleForClaim_WithInactivePolicy_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Cancelled;
            var claimDate = DateTime.Today;

            // Act
            var isEligible = _calculator.IsPolicyEligibleForClaim(policy, claimDate);

            // Assert
            isEligible.Should().BeFalse();
        }

        [Fact]
        public void IsPolicyEligibleForClaim_WithClaimDateBeforeCoverage_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            var claimDate = policy.StartDate.AddDays(-1);

            // Act
            var isEligible = _calculator.IsPolicyEligibleForClaim(policy, claimDate);

            // Assert
            isEligible.Should().BeFalse();
        }

        [Fact]
        public void IsPolicyEligibleForClaim_WithClaimDateAfterCoverage_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            var claimDate = policy.EndDate.AddDays(1);

            // Act
            var isEligible = _calculator.IsPolicyEligibleForClaim(policy, claimDate);

            // Assert
            isEligible.Should().BeFalse();
        }

        [Theory]
        [InlineData(2000, 500, 1500, 1000)]  // Claimed 1500, deductible 500 = 1000 payable
        [InlineData(1000, 200, 800, 600)]    // Claimed 800, deductible 200 = 600 payable
        [InlineData(3000, 0, 2000, 2000)]    // Claimed 2000, no deductible = 2000 payable
        [InlineData(1000, 1500, 2000, 500)]  // Claimed 2000, deductible 1500 = 500 payable (within max 1500)
        public void CalculatePayableAmount_WithVariousScenarios_ShouldCalculateCorrectly(
            decimal premium, decimal deductible, decimal claimed, decimal expectedPayable)
        {
            // Arrange
            var policy = CreateValidPolicy(premiumAmount: premium, deductible: deductible);

            // Act
            var payableAmount = _calculator.CalculatePayableAmount(policy, claimed);

            // Assert
            payableAmount.Should().Be(expectedPayable);
        }

        [Fact]
        public void ValidateClaimAmount_WithIneligiblePolicy_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Expired;
            decimal claimedAmount = 1000m;

            // Act
            var isValid = _calculator.ValidateClaimAmount(policy, claimedAmount);

            // Assert
            isValid.Should().BeFalse();
        }
    }
}
