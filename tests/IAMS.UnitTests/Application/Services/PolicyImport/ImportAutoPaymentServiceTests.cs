using FluentAssertions;
using IAMS.Application.Services.PolicyImport;
using IAMS.Domain.Entities;
using IAMS.Shared.DTOs.Settings;
using IAMS.Shared.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IAMS.UnitTests.Application.Services.PolicyImport;

public class ImportAutoPaymentServiceTests
{
    private static ImportAutoPaymentService CreateService(PolicyImportSettingsDto? settings = null)
    {
        var tenantService = new Mock<ITenantService>();
        tenantService
            .Setup(s => s.GetTenantSettingAsync<PolicyImportSettingsDto>(PolicyImportSettingsDto.SettingKey))
            .ReturnsAsync(settings);

        return new ImportAutoPaymentService(tenantService.Object, NullLogger<ImportAutoPaymentService>.Instance);
    }

    private static PolicyType Type(string? name = null, string? code = null, string? category = null) =>
        new() { Name = name!, Code = code!, Category = category! };

    // The law covers the whole traffic family, in every spelling and casing that
    // appears in source data — including the Turkish dotted İ.
    [Theory]
    [InlineData("Trafik Sigortası")]
    [InlineData("TRAFİK")]
    [InlineData("trafik")]
    [InlineData("Traffic Insurance")]
    [InlineData("Trafic")]
    [InlineData("Kasko")]
    [InlineData("KASKO")]
    [InlineData("Tam Kasko")]
    [InlineData("Yarım Kasko")]
    [InlineData("Half Kasko")]
    public void RequiresFullPaymentByLaw_TrafficFamilyNames_AreDetected(string name)
    {
        CreateService().RequiresFullPaymentByLaw(Type(name: name)).Should().BeTrue();
    }

    [Theory]
    [InlineData("Konut Sigortası")]
    [InlineData("Sağlık")]
    [InlineData("DASK")]
    [InlineData("")]
    public void RequiresFullPaymentByLaw_OtherPolicyTypes_AreNot(string name)
    {
        CreateService().RequiresFullPaymentByLaw(Type(name: name)).Should().BeFalse();
    }

    [Fact]
    public void RequiresFullPaymentByLaw_MatchesOnCodeAndCategoryToo()
    {
        var service = CreateService();

        service.RequiresFullPaymentByLaw(Type(name: "Motorlu Araç", code: "TRF-TRAFIK")).Should().BeTrue();
        service.RequiresFullPaymentByLaw(Type(name: "Motorlu Araç", category: "Kasko Ürünleri")).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldAutoPay_DefaultsToOn_WhenNoSettingStored()
    {
        var service = CreateService(settings: null);

        (await service.ShouldAutoPayAsync(Type(name: "Trafik"))).Should().BeTrue();
    }

    [Fact]
    public async Task ShouldAutoPay_RespectsAgencyOptOut()
    {
        var service = CreateService(new PolicyImportSettingsDto { AutoPayMandatoryPolicies = false });

        (await service.ShouldAutoPayAsync(Type(name: "Trafik"))).Should().BeFalse();
        (await service.ShouldAutoPayAsync(Type(name: "Tam Kasko"))).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldAutoPay_NeverAppliesToNonTrafficPolicies()
    {
        var service = CreateService(new PolicyImportSettingsDto { AutoPayMandatoryPolicies = true });

        (await service.ShouldAutoPayAsync(Type(name: "Konut"))).Should().BeFalse();
    }

    [Fact]
    public async Task ShouldAutoPay_ReadsTheSettingOncePerServiceScope()
    {
        var tenantService = new Mock<ITenantService>();
        tenantService
            .Setup(s => s.GetTenantSettingAsync<PolicyImportSettingsDto>(PolicyImportSettingsDto.SettingKey))
            .ReturnsAsync(new PolicyImportSettingsDto { AutoPayMandatoryPolicies = true });

        var service = new ImportAutoPaymentService(tenantService.Object, NullLogger<ImportAutoPaymentService>.Instance);

        await service.ShouldAutoPayAsync(Type(name: "Trafik"));
        await service.ShouldAutoPayAsync(Type(name: "Kasko"));
        await service.ShouldAutoPayAsync(Type(name: "Trafik"));

        // One import batch = one service scope: the agency setting is read a single time,
        // so toggling it mid-import cannot split a running batch between modes.
        tenantService.Verify(
            s => s.GetTenantSettingAsync<PolicyImportSettingsDto>(PolicyImportSettingsDto.SettingKey),
            Times.Once);
    }
}
