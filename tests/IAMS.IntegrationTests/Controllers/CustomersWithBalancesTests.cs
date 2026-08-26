using System.Net.Http.Json;
using FluentAssertions;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using IAMS.IntegrationTests.Fixtures;
using IAMS.Persistence.Contexts;
using IAMS.Shared.DTOs.Customer;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IAMS.IntegrationTests.API.Controllers;

/// <summary>
/// Covers the SQL-aggregated customers-with-balances endpoint (#517):
/// per-currency premium/paid totals, inclusion of fully-paid customers,
/// and the optional customerId filter.
/// </summary>
public class CustomersWithBalancesTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private const string Tenant = "test-agency-1";
    private readonly TestWebApplicationFactory<Program> _factory;

    public CustomersWithBalancesTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        SeedPoliciesAndPayments();
    }

    private void SeedPoliciesAndPayments()
    {
        using var scope = _factory.Services.CreateScope();
        var tenantAccessor = scope.ServiceProvider.GetRequiredService<IAMS.MultiTenancy.Interfaces.ITenantContextAccessor>();
        tenantAccessor.TenantContext = new IAMS.MultiTenancy.Models.TenantContext(
            new IAMS.MultiTenancy.Models.Tenant { Identifier = Tenant });

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (db.Policies.Any())
        {
            return; // already seeded by an earlier test in this class
        }

        // The context seeds currencies itself; reuse them (create only if missing).
        var tryCurrency = db.Currencies.FirstOrDefault(c => c.Code == "TRY")
            ?? db.Currencies.Add(new Currency { Code = "TRY", Name = "Turkish Lira" }).Entity;
        var usdCurrency = db.Currencies.FirstOrDefault(c => c.Code == "USD")
            ?? db.Currencies.Add(new Currency { Code = "USD", Name = "US Dollar" }).Entity;
        db.SaveChanges();

        // Customer 1 (Ahmet, seeded by DatabaseSeeder): two TRY policies,
        // 1000 with 400 paid + 500 fully paid → premium 1500, paid 900, balance 600.
        var p1 = NewPolicy(id: 101, customerId: 1, currencyId: tryCurrency.Id, premium: 1000m);
        var p2 = NewPolicy(id: 102, customerId: 1, currencyId: tryCurrency.Id, premium: 500m);
        // Customer 2 (Fatma): one USD policy, fully paid → premium 200, balance 0.
        var p3 = NewPolicy(id: 103, customerId: 2, currencyId: usdCurrency.Id, premium: 200m);
        db.Policies.AddRange(p1, p2, p3);

        db.PolicyPayments.AddRange(
            NewPayment(id: 201, policyId: 101, currencyId: tryCurrency.Id, amount: 400m),
            NewPayment(id: 202, policyId: 102, currencyId: tryCurrency.Id, amount: 500m),
            NewPayment(id: 203, policyId: 103, currencyId: usdCurrency.Id, amount: 200m));

        db.SaveChanges();
    }

    private static Policy NewPolicy(int id, int customerId, int currencyId, decimal premium) => new()
    {
        Id = id,
        PolicyNumber = $"POL-{id}",
        CustomerId = customerId,
        CurrencyId = currencyId,
        InsuranceCompanyId = 1,
        PolicyTypeId = 1,
        PremiumAmount = premium,
        Status = PolicyStatus.Active,
        StartDate = DateTime.Today,
        EndDate = DateTime.Today.AddYears(1),
        CreatedOn = DateTime.UtcNow
    };

    private static PolicyPayment NewPayment(int id, int policyId, int currencyId, decimal amount) => new()
    {
        Id = id,
        PolicyId = policyId,
        CurrencyId = currencyId,
        Amount = amount,
        PaymentDate = DateTime.Today,
        Status = PaymentStatus.Completed,
        CreatedOn = DateTime.UtcNow
    };

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-ID", Tenant);
        return client;
    }

    [Fact]
    public async Task ReturnsPremiumAndBalanceTotals_PerCustomerAndCurrency()
    {
        var client = CreateClient();

        var response = await client.GetFromJsonAsync<IAMS.Shared.Models.Result<List<CustomerWithBalanceDto>>>(
            "/api/customers/with-balances");

        response!.IsSuccess.Should().BeTrue();
        var rows = response.Data!;

        var ahmetTry = rows.Single(r => r.Id == 1 && r.Currency == "TRY");
        ahmetTry.TotalPremium.Should().Be(1500m);
        ahmetTry.TotalPaid.Should().Be(900m);
        ahmetTry.Balance.Should().Be(600m);
        ahmetTry.ActivePolicyCount.Should().Be(2);
        ahmetTry.FirstName.Should().Be("Ahmet");
    }

    [Fact]
    public async Task IncludesFullyPaidCustomers_WithZeroBalance()
    {
        var client = CreateClient();

        var response = await client.GetFromJsonAsync<IAMS.Shared.Models.Result<List<CustomerWithBalanceDto>>>(
            "/api/customers/with-balances");

        // Fatma's USD policy is fully paid — before #517 she was omitted entirely;
        // now her premium total must be available with a zero balance.
        var fatmaUsd = response!.Data!.Single(r => r.Id == 2 && r.Currency == "USD");
        fatmaUsd.TotalPremium.Should().Be(200m);
        fatmaUsd.Balance.Should().Be(0m);
    }

    [Fact]
    public async Task CustomerIdFilter_ReturnsOnlyThatCustomer()
    {
        var client = CreateClient();

        var response = await client.GetFromJsonAsync<IAMS.Shared.Models.Result<List<CustomerWithBalanceDto>>>(
            "/api/customers/with-balances?customerId=2");

        response!.IsSuccess.Should().BeTrue();
        response.Data.Should().OnlyContain(r => r.Id == 2);
        response.Data.Should().ContainSingle(r => r.Currency == "USD");
    }
}
