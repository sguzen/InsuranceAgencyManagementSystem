using FluentAssertions;
using IAMS.Shared.Validation;

namespace IAMS.UnitTests.Shared
{
    public class AgencyCodeRulesTests
    {
        [Theory]
        [InlineData("A022")]
        [InlineData("0157")]
        [InlineData("a")]
        [InlineData("ABCDEFGHIJ")] // exactly MaxLength
        public void IsValid_AcceptsAlphanumericUpToMaxLength(string code)
        {
            AgencyCodeRules.IsValid(code).Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ABCDEFGHIJK")] // MaxLength + 1
        [InlineData("A-22")]
        [InlineData("A 22")]
        [InlineData("A022'; DROP TABLE yanpol; --")]
        public void IsValid_RejectsEmptyTooLongOrNonAlphanumeric(string? code)
        {
            AgencyCodeRules.IsValid(code).Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryNormalize_TreatsBlankAsNotConfigured(string? input)
        {
            var ok = AgencyCodeRules.TryNormalize(input, out var normalized, out var error);

            ok.Should().BeTrue();
            normalized.Should().BeNull();
            error.Should().BeNull();
        }

        [Fact]
        public void TryNormalize_TrimsSurroundingWhitespace()
        {
            var ok = AgencyCodeRules.TryNormalize("  A022 ", out var normalized, out var error);

            ok.Should().BeTrue();
            normalized.Should().Be("A022");
            error.Should().BeNull();
        }

        [Fact]
        public void TryNormalize_RejectsMalformedValueWithMessage()
        {
            var ok = AgencyCodeRules.TryNormalize("A-22", out var normalized, out var error);

            ok.Should().BeFalse();
            normalized.Should().BeNull();
            error.Should().Contain("A-22").And.Contain("letters or digits");
        }
    }
}
