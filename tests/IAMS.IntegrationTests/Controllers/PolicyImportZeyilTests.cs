using System.Net.Http.Json;
using FluentAssertions;
using IAMS.Domain.Entities;
using IAMS.IntegrationTests.Fixtures;
using IAMS.Persistence.Contexts;
using IAMS.Shared.DTOs.Policy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAMS.IntegrationTests.API.Controllers;

/// <summary>
/// End-to-end coverage of the zeyil import rules (#528) through the mapping import
/// endpoint, and a first slice of the missing import coverage from #509:
/// - a zeyil without its original imports with a warning and shows as an orphan,
/// - importing the original afterwards links the orphan automatically,
/// - the traffic auto-payment is signed (negative for iade zeyils),
/// - the per-agency AutoPayMandatoryPolicies switch disables synthetic payments.
/// </summary>
public class PolicyImportZeyilTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Tenant = "test-agency-1";
    private const string TrafficTypeCode = "15"; // seeded "ZORUNLU TRAFİK" (tests the Turkish İ folding)
    private const string FireTypeCode = "01";    // seeded "YANGIN" — never auto-paid

    private readonly TestWebApplicationFactory<Program> _factory;

    public PolicyImportZeyilTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string tenant = Tenant)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-ID", tenant);
        return client;
    }

    private static PolicyImportPreviewDto Row(
        string policyNumber, string innerCode, decimal premium, string typeCode = TrafficTypeCode) => new()
    {
        RowNumber = 1,
        PolicyNumber = policyNumber,
        InnerCode = innerCode,
        PremiumAmount = premium,
        PolicyOwnerSameAsInsured = true,
        OriginalImportData = new ImportPolicyDto
        {
            PolicyNumber = policyNumber,
            InnerCode = innerCode,
            PolicyTypeCode = typeCode,
            CurrencyCode = "TRY",
            PremiumAmount = premium,
            CustomerName = "Zeyil Testeri",
            CustomerIdentifier = "555001",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddYears(1)
        }
    };

    private async Task<PolicyImportResultDto> ImportAsync(HttpClient client, params PolicyImportPreviewDto[] rows)
    {
        var response = await client.PostAsJsonAsync("/api/policies/import/with-mapping",
            new { MappedPolicies = rows, InsuranceCompanyId = 1 });
        var body = await response.Content.ReadFromJsonAsync<IAMS.Shared.Models.Result<PolicyImportResultDto>>();
        response.IsSuccessStatusCode.Should().BeTrue(body?.Message ?? "import request failed");
        return body!.Data!;
    }

    private T InTenantDb<T>(Func<ApplicationDbContext, T> query, string tenant = Tenant)
    {
        using var scope = _factory.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IAMS.MultiTenancy.Interfaces.ITenantContextAccessor>();
        accessor.TenantContext = new IAMS.MultiTenancy.Models.TenantContext(
            new IAMS.MultiTenancy.Models.Tenant { Identifier = tenant });
        return query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    [Fact]
    public async Task OrphanZeyil_Imports_WithWarning_ThenLinksWhenOriginalArrives()
    {
        var client = CreateClient();

        // 1. Zeyil first — its original does not exist yet
        var zeyilResult = await ImportAsync(client, Row("TRF-9001", "001", 500m));
        zeyilResult.SuccessCount.Should().Be(1);
        zeyilResult.Warnings.Should().ContainSingle(w => w.Contains("TRF-9001"));

        var orphan = InTenantDb(db => db.Policies.Single(p => p.PolicyNumber == "TRF-9001" && p.InnerCode == "001"));
        orphan.OriginalPolicyId.Should().BeNull("the original is not in the system yet");

        // The auto-pay rule (default ON) also covers the orphan
        InTenantDb(db => db.PolicyPayments.Single(p => p.PolicyId == orphan.Id).Amount).Should().Be(500m);

        // 2. Original arrives later — the orphan must be linked automatically
        var originalResult = await ImportAsync(client, Row("TRF-9001", "000", 1000m));
        originalResult.SuccessCount.Should().Be(1);
        originalResult.Warnings.Should().BeEmpty();

        var original = InTenantDb(db => db.Policies.Single(p => p.PolicyNumber == "TRF-9001" && p.InnerCode == "000"));
        var linked = InTenantDb(db => db.Policies.Single(p => p.Id == orphan.Id));
        linked.OriginalPolicyId.Should().Be(original.Id, "importing the original links existing orphan zeyils");

        // Chain balance: 1000 + 500 premiums, 1000 + 500 auto-payments → zero outstanding
        var totals = InTenantDb(db => new
        {
            Premium = db.Policies.Where(p => p.PolicyNumber == "TRF-9001").Sum(p => p.PremiumAmount),
            Paid = db.PolicyPayments.Where(pp => pp.Policy.PolicyNumber == "TRF-9001").Sum(pp => pp.Amount)
        });
        totals.Paid.Should().Be(totals.Premium);
    }

    [Fact]
    public async Task NegativeZeyil_CreatesNegativeAutoPayment_KeepingChainAtZero()
    {
        var client = CreateClient();

        await ImportAsync(client, Row("TRF-9002", "000", 1000m));
        var result = await ImportAsync(client, Row("TRF-9002", "001", -200m));
        result.SuccessCount.Should().Be(1);

        var payments = InTenantDb(db => db.PolicyPayments
            .Where(pp => pp.Policy.PolicyNumber == "TRF-9002")
            .Select(pp => pp.Amount)
            .ToList());

        payments.Should().BeEquivalentTo(new[] { 1000m, -200m },
            "an iade zeyil gets a matching negative payment so the chain stays at zero balance");
    }

    [Fact]
    public async Task NonTrafficPolicy_GetsNoAutoPayment()
    {
        var client = CreateClient();

        await ImportAsync(client, Row("YAN-9003", "000", 750m, FireTypeCode));

        InTenantDb(db => db.PolicyPayments.Any(pp => pp.Policy.PolicyNumber == "YAN-9003"))
            .Should().BeFalse("YANGIN is not in the traffic family");
    }

    [Fact]
    public async Task AutoPaySwitchOff_ImportsTrafficPolicy_WithoutSyntheticPayment()
    {
        // test-agency-2 gets its own database with the auto-pay rule switched off,
        // so this cannot interfere with the rule-ON tests above.
        InTenantDb(db =>
        {
            db.Database.EnsureCreated(); // seed policy types/currencies for this tenant DB
            if (!db.TenantSettings.Any(s => s.SettingKey == "policyImport"))
            {
                db.TenantSettings.Add(new TenantSettings
                {
                    SettingKey = "policyImport",
                    SettingValue = "{\"AutoPayMandatoryPolicies\":false}",
                    CreatedOn = DateTime.UtcNow
                });
                db.SaveChanges();
            }
            return 0;
        }, tenant: "test-agency-2");

        var client = CreateClient(tenant: "test-agency-2");
        var result = await ImportAsync(client, Row("TRF-9004", "000", 900m));
        result.SuccessCount.Should().Be(1);

        InTenantDb(db => db.PolicyPayments.Any(pp => pp.Policy.PolicyNumber == "TRF-9004"), tenant: "test-agency-2")
            .Should().BeFalse("the agency turned AutoPayMandatoryPolicies off");
    }
}
