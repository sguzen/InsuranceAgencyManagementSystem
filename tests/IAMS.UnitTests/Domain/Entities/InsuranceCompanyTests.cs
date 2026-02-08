using FluentAssertions;
using IAMS.Domain.Entities;

namespace IAMS.UnitTests.Domain.Entities
{
    public class InsuranceCompanyTests
    {
        [Fact]
        public void Create_WithValidName_ShouldCreateInsuranceCompanyWithCode()
        {
            // Arrange
            var name = "AXA Sigorta";
            var description = "Test insurance company";
            var email = "test@axa.com";
            var phone = "555-1234";
            var address = "Test Address";
            var website = "www.axa.com";
            var createdBy = "TestUser";

            // Act
            var company = InsuranceCompany.Create(name, description, email, phone, address, website, createdBy);

            // Assert
            company.Should().NotBeNull();
            company.Name.Should().Be(name);
            company.Code.Should().NotBeNullOrEmpty();
            company.Code.Length.Should().Be(10); // 3-char prefix + 7-digit timestamp
            company.Code.Should().StartWith("AXA");
            company.Description.Should().Be(description);
            company.Email.Should().Be(email);
            company.Phone.Should().Be(phone);
            company.Address.Should().Be(address);
            company.Website.Should().Be(website);
            company.IsActive.Should().BeTrue();
            company.CreatedBy.Should().Be(createdBy);
        }

        [Fact]
        public void Create_WithEmptyName_ShouldThrowException()
        {
            // Arrange
            var name = "";

            // Act & Assert
            var act = () => InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Insurance company name is required*");
        }

        [Fact]
        public void Create_WithWhitespaceName_ShouldThrowException()
        {
            // Arrange
            var name = "   ";

            // Act & Assert
            var act = () => InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Insurance company name is required*");
        }

        [Fact]
        public void Create_WithShortName_ShouldGenerateCodeWithPaddedPrefix()
        {
            // Arrange
            var name = "AB"; // Only 2 characters

            // Act
            var company = InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");

            // Assert
            company.Code.Should().NotBeNullOrEmpty();
            company.Code.Length.Should().Be(10);
            company.Code.Should().StartWith("ABX"); // Padded with 'X'
        }

        [Fact]
        public void Create_WithSingleCharacterName_ShouldGenerateCodeWithPaddedPrefix()
        {
            // Arrange
            var name = "A";

            // Act
            var company = InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");

            // Assert
            company.Code.Should().NotBeNullOrEmpty();
            company.Code.Length.Should().Be(10);
            company.Code.Should().StartWith("AXX"); // Padded with 'XX'
        }

        [Fact]
        public void Create_WithSpecialCharactersInName_ShouldUseDefaultPrefix()
        {
            // Arrange
            var name = "!!!"; // Only special characters

            // Act
            var company = InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");

            // Assert
            company.Code.Should().NotBeNullOrEmpty();
            company.Code.Length.Should().Be(10);
            company.Code.Should().StartWith("INS"); // Default prefix
        }

        [Fact]
        public void Create_WithLongName_ShouldUseFirst3Characters()
        {
            // Arrange
            var name = "ABCDEFGHIJ Insurance Company";

            // Act
            var company = InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");

            // Assert
            company.Code.Should().NotBeNullOrEmpty();
            company.Code.Length.Should().Be(10);
            company.Code.Should().StartWith("ABC");
        }

        [Fact]
        public void Create_MultipleTimes_ShouldGenerateUniqueCodes()
        {
            // Arrange
            var name = "Test Insurance";
            var codes = new HashSet<string>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                var company = InsuranceCompany.Create(name, null, null, null, null, null, "TestUser");
                codes.Add(company.Code);
                Thread.Sleep(1); // Ensure different timestamps
            }

            // Assert
            codes.Should().HaveCountGreaterThan(1, "codes should be unique due to timestamp");
        }

        [Fact]
        public void Activate_WithDeactivatedCompany_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var company = InsuranceCompany.Create("Test", null, null, null, null, null, "TestUser");
            company.IsActive = false;

            // Act
            company.Activate("TestUser");

            // Assert
            company.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_WithActiveCompany_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var company = InsuranceCompany.Create("Test", null, null, null, null, null, "TestUser");

            // Act
            company.Deactivate("TestUser");

            // Assert
            company.IsActive.Should().BeFalse();
        }

        [Fact]
        public void HasMasterLink_WithMasterInsuranceCompanyId_ShouldReturnTrue()
        {
            // Arrange
            var company = InsuranceCompany.Create("Test", null, null, null, null, null, "TestUser");
            company.MasterInsuranceCompanyId = 1;

            // Act
            var result = company.HasMasterLink;

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasMasterLink_WithoutMasterInsuranceCompanyId_ShouldReturnFalse()
        {
            // Arrange
            var company = InsuranceCompany.Create("Test", null, null, null, null, null, "TestUser");

            // Act
            var result = company.HasMasterLink;

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetCommissionRate_WithMatchingPolicyType_ShouldReturnActiveRate()
        {
            // Arrange
            var company = InsuranceCompany.Create("Test", null, null, null, null, null, "TestUser");
            var policyTypeId = 1;
            var effectiveDate = DateTime.Today;

            var commissionRate1 = new CommissionRate
            {
                PolicyTypeId = policyTypeId,
                Rate = 10m,
                IsActive = true,
                EffectiveDate = DateTime.Today.AddMonths(-1),
                ExpiryDate = null
            };

            var commissionRate2 = new CommissionRate
            {
                PolicyTypeId = policyTypeId,
                Rate = 8m,
                IsActive = false,
                EffectiveDate = DateTime.Today.AddMonths(-2),
                ExpiryDate = DateTime.Today.AddMonths(-1)
            };

            company.CommissionRates = new List<CommissionRate> { commissionRate1, commissionRate2 };

            // Act
            var result = company.GetCommissionRate(policyTypeId, effectiveDate);

            // Assert
            result.Should().NotBeNull();
            result.Rate.Should().Be(10m);
        }

        [Fact]
        public void GetCommissionRate_WithNoMatchingRate_ShouldReturnNull()
        {
            // Arrange
            var company = InsuranceCompany.Create("Test", null, null, null, null, null, "TestUser");
            var policyTypeId = 99; // Non-existent policy type

            // Act
            var result = company.GetCommissionRate(policyTypeId, DateTime.Today);

            // Assert
            result.Should().BeNull();
        }
    }
}
