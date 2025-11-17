using FluentAssertions;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces;
using IAMS.IntegrationTests.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace IAMS.IntegrationTests.API.Controllers;

public class VehiclesControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;

    public VehiclesControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithMockedService(Mock<IVehicleDataService> mockService)
    {
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDataService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add mock
                services.AddScoped(_ => mockService.Object);
            });
        }).CreateClient();

        // Add tenant header for multi-tenancy
        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

        return client;
    }

    [Fact]
    public async Task TestConnection_WhenServiceIsAvailable_ReturnsSuccess()
    {
        // Arrange
        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.TestConnectionAsync()).ReturnsAsync(true);

        var client = CreateClientWithMockedService(mockService);

        // Act
        var response = await client.GetAsync("/api/vehicles/test-connection");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TestConnection_WhenServiceIsUnavailable_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.TestConnectionAsync()).ReturnsAsync(false);

        var client = CreateClientWithMockedService(mockService);

        // Act
        var response = await client.GetAsync("/api/vehicles/test-connection");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task FetchExternalData_WhenDataExists_ReturnsData()
    {
        // Arrange
        var expectedData = new List<ExternalVehicleDataDto>
        {
            new ExternalVehicleDataDto
            {
                Id = 1,
                BrandCode = 100,
                BrandName = "Toyota",
                ModelName = "Corolla",
                BrandModelName = "Toyota Corolla"
            }
        };

        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.FetchVehicleDataAsync()).ReturnsAsync(expectedData);

        var client = CreateClientWithMockedService(mockService);

        // Act
        var response = await client.GetAsync("/api/vehicles/fetch-external-data");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FetchExternalData_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.FetchVehicleDataAsync())
            .ThrowsAsync(new HttpRequestException("External API is down"));

        var client = CreateClientWithMockedService(mockService);

        // Act
        var response = await client.GetAsync("/api/vehicles/fetch-external-data");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SyncVehicleData_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var externalData = new List<ExternalVehicleDataDto>
        {
            new ExternalVehicleDataDto
            {
                Id = 1,
                BrandCode = 100,
                BrandName = "Toyota",
                ModelName = "Corolla",
                BrandModelName = "Toyota Corolla"
            }
        };

        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.FetchVehicleDataAsync()).ReturnsAsync(externalData);

        var client = CreateClientWithMockedService(mockService);

        // Act
        var response = await client.PostAsync("/api/vehicles/sync?updateExisting=true&deactivateMissing=false", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncVehicleData_WhenServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.FetchVehicleDataAsync())
            .ThrowsAsync(new HttpRequestException("External API is down"));

        var client = CreateClientWithMockedService(mockService);

        // Act
        var response = await client.PostAsync("/api/vehicles/sync", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
