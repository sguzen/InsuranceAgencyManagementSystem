using FluentAssertions;
using IAMS.Application.DTOs.Customer;
using IAMS.IntegrationTests.Fixtures;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
        _client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");
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
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();

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
        var createDto = new CreateCustomerDto
        {
            FirstName = "Mehmet",
            LastName = "Yılmaz",
            Email = "mehmet.yilmaz@example.com",
            Phone = "+90 533 111 2233"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        customer.Should().NotBeNull();
        customer!.FirstName.Should().Be("Mehmet");
        customer.LastName.Should().Be("Yılmaz");
        customer.Email.Should().Be("mehmet.yilmaz@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateCustomer_WithInvalidFirstName_ReturnsBadRequest(string firstName)
    {
        // Arrange
        var createDto = new CreateCustomerDto
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