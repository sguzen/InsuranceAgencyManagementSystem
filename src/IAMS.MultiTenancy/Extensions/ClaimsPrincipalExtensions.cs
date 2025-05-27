using System.Security.Claims;

namespace IAMS.MultiTenancy.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetTenantId(this ClaimsPrincipal principal)
        {
            var tenantIdClaim = principal.FindFirst("tenant_id");
            if (tenantIdClaim != null && int.TryParse(tenantIdClaim.Value, out int tenantId))
            {
                return tenantId;
            }
            return null;
        }

        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}