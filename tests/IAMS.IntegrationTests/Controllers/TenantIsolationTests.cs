using FluentAssertions;
using IAMS.Shared.DTOs.Customer;
using IAMS.IntegrationTests.Fixtures;

using System.Net.Http.Json;

namespace IAMS.IntegrationTests.MultiTenancy;

public class TenantIsolationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public TenantIsolationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CustomerData_IsolatedBetweenTenants()
    {
        // Arrange
        var tenant1Client = _factory.CreateClient();
        var tenant2Client = _factory.CreateClient();

        tenant1Client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");
        tenant2Client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-2");

        var customerDto = new CreateOrUpdateCustomerDto
        {
            FirstName = "Tenant1",
            LastName = "Customer",
            Email = "tenant1@example.com"
        };

        // Act - Create customer in tenant 1
        var createResponse = await tenant1Client.PostAsJsonAsync("/api/customers", customerDto);
        createResponse.EnsureSuccessStatusCode();
        var createdCustomer = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        // Try to access the same customer from tenant 2
        var tenant2Response = await tenant2Client.GetAsync($"/api/customers/{createdCustomer!.Id}");

        // Assert
        tenant2Response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestWithoutTenantHeader_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        // Don't add tenant header

        // Act
        var response = await client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}