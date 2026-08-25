using FluentAssertions;
using IAMS.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IAMS.IntegrationTests.MultiTenancy;

/// <summary>
/// Regression tests for the client-controlled tenant vulnerability (#503):
/// an authenticated user of tenant A must not be able to reach tenant B's data,
/// whatever tenant identifier they put in headers or the query string.
/// </summary>
public class TenantSpoofingTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public TenantSpoofingTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Request_WithTenantHeaderDifferentFromTokenTenant_IsForbidden()
    {
        var client = _factory.CreateClient();
        // Principal "belongs" to test-agency-1 (simulated tenant_id claim in the token)...
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantClaimHeader, "test-agency-1");
        // ...but asks for test-agency-2 via the header.
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "test-agency-2");

        var response = await client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Request_WithMatchingTenantClaim_Succeeds()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantClaimHeader, "test-agency-1");
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "test-agency-1");

        var response = await client.GetAsync("/api/customers");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Request_WithoutHeader_ResolvesTenantFromTokenClaim()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantClaimHeader, "test-agency-1");

        var response = await client.GetAsync("/api/customers");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task QueryStringTenant_IsIgnored()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantClaimHeader, "test-agency-1");

        // If ?tenant= were still honored this would resolve test-agency-2 and the
        // claim check would return 403; instead the query string must be ignored.
        var response = await client.GetAsync("/api/customers?tenant=test-agency-2");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TenantHeader_FromUntrustedCaller_IsIgnored()
    {
        // Production configuration: client tenant resolution disabled. The Test principal
        // has no API-key claim, so its X-Tenant-ID header must not select the tenant.
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "MultiTenancy:AllowClientTenantResolution", "false" }
                });
            });
        });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.TenantClaimHeader, "test-agency-1");
        client.DefaultRequestHeaders.Add("X-Tenant-ID", "test-agency-2");

        // The header is ignored, the tenant comes from the token claim, and the request
        // proceeds against the caller's own tenant instead of the spoofed one.
        var response = await client.GetAsync("/api/customers");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
