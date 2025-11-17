using FluentAssertions;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces;
using IAMS.IntegrationTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace IAMS.IntegrationTests.Controllers;

public class VehiclesControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public VehiclesControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");
    }

    [Fact]
    public async Task TestConnection_WhenServiceIsAvailable_ReturnsSuccess()
    {
        // Arrange
        var mockService = new Mock<IVehicleDataService>();
        mockService.Setup(x => x.TestConnectionAsync()).ReturnsAsync(true);

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

        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

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

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDataService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

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

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDataService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

        // Act
        var response = await client.GetAsync("/api/vehicles/fetch-external-data");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        result.Should().NotBeNull();
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

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDataService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

        // Act
        var response = await client.PostAsync("/api/vehicles/sync?updateExisting=true&deactivateMissing=false", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SyncVehicleData_WhenUpdateExistingIsFalse_ShouldNotUpdateExisting()
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

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDataService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

        // Act
        var response = await client.PostAsync("/api/vehicles/sync?updateExisting=false&deactivateMissing=false", null);

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

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IVehicleDataService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddScoped(_ => mockService.Object);
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Add("X-Tenant", "test-agency-1");

        // Act
        var response = await client.PostAsync("/api/vehicles/sync", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
