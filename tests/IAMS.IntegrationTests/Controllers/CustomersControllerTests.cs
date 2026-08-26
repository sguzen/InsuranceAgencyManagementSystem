using FluentAssertions;
using IAMS.Shared.DTOs.Customer;
using IAMS.IntegrationTests.Fixtures;

using System.Net;
using System.Net.Http.Json;

namespace IAMS.IntegrationTests.API.Controllers;

public class CustomersControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomersControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Add tenant header for multi-tenancy
        _client.DefaultRequestHeaders.Add("X-Tenant-ID", "test-agency-1");
    }

    [Fact]
    public async Task GetCustomers_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType?.ToString().Should().Be("application/json; charset=utf-8");
    }

    [Fact]
    public async Task GetCustomer_WithValidId_ReturnsCustomer()
    {
        // Act
        var response = await _client.GetAsync("/api/customers/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<IAMS.Shared.Models.Result<CustomerDto>>();

        result.Should().NotBeNull();
        var customer = result!.Data;
        customer.Should().NotBeNull();
        customer!.Id.Should().Be(1);
        customer.FirstName.Should().Be("Ahmet");
        customer.LastName.Should().Be("Özkan");
    }

    [Fact]
    public async Task GetCustomer_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/customers/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateOrUpdateCustomerDto
        {
            FirstName = "Mehmet",
            LastName = "Yılmaz",
            Email = "mehmet.yilmaz@example.com",
            MobilePhoneNumber = "+90 533 111 2233"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<IAMS.Shared.Models.Result<CustomerDto>>();

        result.Should().NotBeNull();
        var customer = result!.Data;
        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("Mehmet");
        customer.LastName.Should().Be("Yılmaz");
        customer.Email.Should().Be("mehmet.yilmaz@example.com");
    }

    [Fact]
    public async Task CreateCustomer_WithoutEmail_AutoGeneratesPlaceholder()
    {
        // E-posta is hidden in the UI (#521); a unique placeholder must be generated
        // server-side so the unique index on Email is never violated.
        var createDto = new CreateOrUpdateCustomerDto
        {
            FirstName = "Ayşe",
            LastName = "Kaya",
            MobilePhoneNumber = "+90 533 444 5566"
        };

        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<IAMS.Shared.Models.Result<CustomerDto>>();
        result!.Data!.Email.Should().StartWith("noemail_").And.EndWith("@temp.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateCustomer_WithInvalidFirstName_ReturnsBadRequest(string firstName)
    {
        // Arrange
        var createDto = new CreateOrUpdateCustomerDto
        {
            FirstName = firstName,
            LastName = "Test",
            Email = "test@example.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}