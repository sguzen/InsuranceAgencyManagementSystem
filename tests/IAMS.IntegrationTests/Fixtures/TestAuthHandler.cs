using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace IAMS.IntegrationTests.Fixtures;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <summary>
    /// Request header that makes the test principal carry a tenant_id claim,
    /// simulating a JWT issued for that tenant.
    /// </summary>
    public const string TenantClaimHeader = "X-Test-Tenant-Claim";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        if (Request.Headers.TryGetValue(TenantClaimHeader, out var tenantClaim) &&
            !string.IsNullOrEmpty(tenantClaim.FirstOrDefault()))
        {
            claims.Add(new Claim(IAMS.Shared.Constants.ApplicationConstants.ClaimTypes.TenantId, tenantClaim.First()!));
        }

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
