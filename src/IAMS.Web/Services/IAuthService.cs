using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;

namespace IAMS.Web.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            // Set API base URL (adjust as needed)
            _httpClient.BaseAddress = new Uri("https://localhost:44390/");
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var loginRequest = new
                {
                    Email = email,
                    Password = password
                };

                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Login failed for {Email}: {StatusCode}", email, response.StatusCode);
                    return false;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (authResult == null)
                {
                    _logger.LogError("Failed to deserialize login response");
                    return false;
                }

                // Create claims for cookie authentication
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, authResult.UserId),
                    new(ClaimTypes.Email, authResult.Email),
                    new(ClaimTypes.GivenName, authResult.FirstName),
                    new(ClaimTypes.Surname, authResult.LastName),
                    new(ClaimTypes.Name, $"{authResult.FirstName} {authResult.LastName}".Trim())
                };

                // Add roles
                foreach (var role in authResult.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Add permissions
                foreach (var permission in authResult.Permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }

                // Create identity and sign in
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    await httpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        principal,
                        new AuthenticationProperties
                        {
                            IsPersistent = true,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                        });
                }

                _logger.LogInformation("User {Email} logged in successfully", email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", email);
                //return false;
                throw; // Re-throw to see what's really happening
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
                _logger.LogInformation("User logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }

    // Simple response models
    public class AuthResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string JwtToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}