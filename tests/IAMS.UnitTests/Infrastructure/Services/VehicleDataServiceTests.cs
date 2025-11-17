using FluentAssertions;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace IAMS.UnitTests.Infrastructure.Services
{
    public class VehicleDataServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<VehicleDataService>> _loggerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public VehicleDataServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<VehicleDataService>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            // Setup default configuration
            _configurationMock.Setup(x => x["VehicleDataApi:BaseUrl"])
                .Returns("https://test.websuresoft.com/das");
            _configurationMock.Setup(x => x["VehicleDataApi:TimeoutSeconds"])
                .Returns("30");
        }

        private VehicleDataService CreateService()
        {
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://test.websuresoft.com/das")
            };

            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            return new VehicleDataService(
                _httpClientFactoryMock.Object,
                _loggerMock.Object,
                _configurationMock.Object);
        }

        [Fact]
        public async Task FetchVehicleDataAsync_WhenApiReturnsData_ShouldReturnParsedData()
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

            var jsonResponse = JsonSerializer.Serialize(expectedData);
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            var result = await service.FetchVehicleDataAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].BrandName.Should().Be("Toyota");
            result[0].ModelName.Should().Be("Corolla");
        }

        [Fact]
        public async Task FetchVehicleDataAsync_WhenApiReturnsEmptyArray_ShouldReturnEmptyList()
        {
            // Arrange
            var jsonResponse = "[]";
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            var result = await service.FetchVehicleDataAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchVehicleDataAsync_WhenApiReturnsError_ShouldThrowHttpRequestException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            Func<Task> act = async () => await service.FetchVehicleDataAsync();

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("*500*");
        }

        [Fact]
        public async Task FetchVehicleDataAsync_WhenApiReturnsInvalidJson_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            Func<Task> act = async () => await service.FetchVehicleDataAsync();

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*parse*");
        }

        [Fact]
        public async Task TestConnectionAsync_WhenApiIsAvailable_ShouldReturnTrue()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task TestConnectionAsync_WhenApiIsUnavailable_ShouldReturnFalse()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TestConnectionAsync_WhenNetworkError_ShouldReturnFalse()
        {
            // Arrange
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var service = CreateService();

            // Act
            var result = await service.TestConnectionAsync();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task FetchVehicleDataAsync_ShouldUseCorrectEndpoint()
        {
            // Arrange
            HttpRequestMessage? capturedRequest = null;
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]", System.Text.Encoding.UTF8, "application/json")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
                .ReturnsAsync(httpResponse);

            var service = CreateService();

            // Act
            await service.FetchVehicleDataAsync();

            // Assert
            capturedRequest.Should().NotBeNull();
            capturedRequest!.RequestUri!.ToString().Should().Contain("/dasapi/AracMarkaModel/GetAllBrandAndModels");
            capturedRequest.Method.Should().Be(HttpMethod.Get);
        }
    }
}
