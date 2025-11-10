using FluentAssertions;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.Domain.Exceptions;

namespace IAMS.UnitTests.Domain.Entities
{
    public class PolicyTests
    {
        [Fact]
        public void CreateRenewal_WithValidData_ShouldCreateRenewalPolicyWithParentPolicyId()
        {
            // Arrange
            var parentPolicy = CreateValidPolicy();
            parentPolicy.Status = PolicyStatus.Active;
            var newStartDate = DateTime.Today.AddDays(1);
            var newEndDate = DateTime.Today.AddYears(1);
            var newPremiumAmount = 1500m;
            var renewedBy = "TestUser";

            // Act
            var renewalPolicy = parentPolicy.CreateRenewal(newStartDate, newEndDate, newPremiumAmount, renewedBy);

            // Assert
            renewalPolicy.Should().NotBeNull();
            renewalPolicy.ParentPolicyId.Should().Be(parentPolicy.Id);
            renewalPolicy.StartDate.Should().Be(newStartDate);
            renewalPolicy.EndDate.Should().Be(newEndDate);
            renewalPolicy.PremiumAmount.Should().Be(newPremiumAmount);
            renewalPolicy.Status.Should().Be(PolicyStatus.Draft);
            renewalPolicy.CustomerId.Should().Be(parentPolicy.CustomerId);
            renewalPolicy.InsuranceCompanyId.Should().Be(parentPolicy.InsuranceCompanyId);
            renewalPolicy.PolicyTypeId.Should().Be(parentPolicy.PolicyTypeId);
            renewalPolicy.CommissionRate.Should().Be(parentPolicy.CommissionRate);
        }

        [Fact]
        public void CreateRenewal_WithExpiredPolicy_ShouldCreateRenewalSuccessfully()
        {
            // Arrange
            var parentPolicy = CreateValidPolicy();
            parentPolicy.Status = PolicyStatus.Expired;
            var newStartDate = DateTime.Today.AddDays(1);
            var newEndDate = DateTime.Today.AddYears(1);
            var newPremiumAmount = 1500m;

            // Act
            var renewalPolicy = parentPolicy.CreateRenewal(newStartDate, newEndDate, newPremiumAmount, "TestUser");

            // Assert
            renewalPolicy.Should().NotBeNull();
            renewalPolicy.ParentPolicyId.Should().Be(parentPolicy.Id);
        }

        [Fact]
        public void CreateRenewal_WithCancelledPolicy_ShouldThrowException()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Cancelled;

            // Act & Assert
            var act = () => policy.CreateRenewal(DateTime.Today.AddDays(1), DateTime.Today.AddYears(1), 1500m, "TestUser");
            act.Should().Throw<InvalidOperationDomainException>()
                .WithMessage("*Only active or expired policies can be renewed*");
        }

        [Fact]
        public void CreateRenewal_WithInvalidDateRange_ShouldThrowException()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;
            var startDate = DateTime.Today.AddYears(1);
            var endDate = DateTime.Today.AddDays(1); // End before start

            // Act & Assert
            var act = () => policy.CreateRenewal(startDate, endDate, 1500m, "TestUser");
            act.Should().Throw<BusinessRuleViolationException>()
                .WithMessage("*Renewal start date must be before end date*");
        }

        [Fact]
        public void CreateRenewal_ShouldCalculateCommissionBasedOnParentCommissionRate()
        {
            // Arrange
            var parentPolicy = CreateValidPolicy();
            parentPolicy.Status = PolicyStatus.Active;
            parentPolicy.CommissionRate = 10m; // 10%
            var newPremiumAmount = 2000m;

            // Act
            var renewalPolicy = parentPolicy.CreateRenewal(
                DateTime.Today.AddDays(1),
                DateTime.Today.AddYears(1),
                newPremiumAmount,
                "TestUser");

            // Assert
            renewalPolicy.CommissionRate.Should().Be(10m);
            renewalPolicy.CommissionAmount.Should().Be(200m); // 10% of 2000
        }

        [Fact]
        public void ActivatePolicy_ShouldSetStatusToActive()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Draft;
            policy.StartDate = DateTime.Today.AddDays(-1); // Past start date

            // Act
            policy.ActivatePolicy("TestUser");

            // Assert
            policy.Status.Should().Be(PolicyStatus.Active);
        }

        [Fact]
        public void ActivatePolicy_WithFutureStartDate_ShouldThrowException()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Draft;
            policy.StartDate = DateTime.Today.AddDays(10);

            // Act & Assert
            var act = () => policy.ActivatePolicy("TestUser");
            act.Should().Throw<BusinessRuleViolationException>()
                .WithMessage("*Cannot activate a policy with a future start date*");
        }

        [Fact]
        public void CancelPolicy_WithActivePolicy_ShouldSetStatusToCancelled()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;

            // Act
            policy.CancelPolicy("TestUser", "Customer request");

            // Assert
            policy.Status.Should().Be(PolicyStatus.Cancelled);
        }

        [Fact]
        public void SuspendPolicy_WithActivePolicy_ShouldSetStatusToSuspended()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;

            // Act
            policy.SuspendPolicy("TestUser", "Non-payment");

            // Assert
            policy.Status.Should().Be(PolicyStatus.Suspended);
        }

        [Fact]
        public void ReactivatePolicy_WithSuspendedPolicy_ShouldSetStatusToActive()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Suspended;
            policy.EndDate = DateTime.Today.AddMonths(6); // Not expired

            // Act
            policy.ReactivatePolicy("TestUser");

            // Assert
            policy.Status.Should().Be(PolicyStatus.Active);
        }

        [Fact]
        public void ReactivatePolicy_WithExpiredEndDate_ShouldThrowException()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Suspended;
            policy.EndDate = DateTime.Today.AddDays(-1); // Already expired

            // Act & Assert
            var act = () => policy.ReactivatePolicy("TestUser");
            act.Should().Throw<BusinessRuleViolationException>()
                .WithMessage("*Cannot reactivate an expired policy*");
        }

        [Fact]
        public void ExpirePolicy_WithActivePolicyPastEndDate_ShouldSetStatusToExpired()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;
            policy.EndDate = DateTime.Today.AddDays(-1);

            // Act
            policy.ExpirePolicy();

            // Assert
            policy.Status.Should().Be(PolicyStatus.Expired);
        }

        [Fact]
        public void IsExpiring_WithPolicyExpiringIn15Days_ShouldReturnTrue()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;
            policy.EndDate = DateTime.Today.AddDays(15);

            // Act
            var result = policy.IsExpiring(30);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsExpiring_WithPolicyExpiringIn45Days_ShouldReturnFalse()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;
            policy.EndDate = DateTime.Today.AddDays(45);

            // Act
            var result = policy.IsExpiring(30);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WithPastEndDate_ShouldReturnTrue()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.EndDate = DateTime.Today.AddDays(-1);

            // Act
            var result = policy.IsExpired();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void UpdatePremium_WithValidAmount_ShouldUpdatePremiumAndRecalculateCommission()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.Status = PolicyStatus.Active;
            policy.CommissionRate = 10m;
            var newPremiumAmount = 3000m;

            // Act
            policy.UpdatePremium(newPremiumAmount, "TestUser");

            // Assert
            policy.PremiumAmount.Should().Be(3000m);
            policy.CommissionAmount.Should().Be(300m); // 10% of 3000
        }

        [Fact]
        public void UpdateCommissionRate_WithValidRate_ShouldUpdateRateAndRecalculateCommission()
        {
            // Arrange
            var policy = CreateValidPolicy();
            policy.PremiumAmount = 2000m;
            policy.CommissionRate = 10m;
            policy.CommissionAmount = 200m;

            // Act
            policy.UpdateCommissionRate(15m, "TestUser");

            // Assert
            policy.CommissionRate.Should().Be(15m);
            policy.CommissionAmount.Should().Be(300m); // 15% of 2000
        }

        [Fact]
        public void UpdateCommissionRate_WithInvalidRate_ShouldThrowException()
        {
            // Arrange
            var policy = CreateValidPolicy();

            // Act & Assert
            var act = () => policy.UpdateCommissionRate(150m, "TestUser");
            act.Should().Throw<BusinessRuleViolationException>()
                .WithMessage("*Commission rate must be between 0 and 100*");
        }

        private Policy CreateValidPolicy()
        {
            return new Policy
            {
                Id = 1,
                PolicyNumber = "TEST-001",
                CustomerId = 1,
                InsuranceCompanyId = 1,
                PolicyTypeId = 1,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                PremiumAmount = 1000m,
                CommissionRate = 10m,
                CommissionAmount = 100m,
                Status = PolicyStatus.Draft,
                CurrencyId = 1,
                CreatedBy = "System",
                CreatedOn = DateTime.UtcNow
            };
        }
    }
}
