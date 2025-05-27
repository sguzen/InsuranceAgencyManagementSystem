using IAMS.Application.DTOs.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IAMS.IntegrationTests.Controllers
{
    // IAMS.IntegrationTests/Controllers/AuthControllerTests.cs
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var loginDto = new
            {
                Email = "admin@tenant1.com",
                Password = "Admin@123456"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponseDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.NotEmpty(result.JwtToken);
            Assert.NotEmpty(result.RefreshToken);
            Assert.Equal("admin@tenant1.com", result.Email);
            Assert.Contains("Administrator", result.Roles);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new
            {
                Email = "admin@tenant1.com",
                Password = "WrongPassword"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginDto),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewToken()
        {
            // First login to get a token
            var loginDto = new
            {
                Email = "admin@tenant1.com",
                Password = "Admin@123456"
            };

            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginDto),
                Encoding.UTF8,
                "application/json");

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();

            var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<AuthResponseDto>(loginResponseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Now try to refresh the token
            var refreshDto = new
            {
                RefreshToken = loginResult.RefreshToken
            };

            var refreshContent = new StringContent(
                JsonSerializer.Serialize(refreshDto),
                Encoding.UTF8,
                "application/json");

            var refreshResponse = await _client.PostAsync("/api/auth/refresh-token", refreshContent);

            // Assert
            refreshResponse.EnsureSuccessStatusCode();

            var refreshResponseString = await refreshResponse.Content.ReadAsStringAsync();
            var refreshResult = JsonSerializer.Deserialize<AuthResponseDto>(refreshResponseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(refreshResult);
            Assert.NotEmpty(refreshResult.JwtToken);
            Assert.NotEmpty(refreshResult.RefreshToken);
            Assert.NotEqual(loginResult.JwtToken, refreshResult.JwtToken);
            Assert.NotEqual(loginResult.RefreshToken, refreshResult.RefreshToken);
        }
    }
}
