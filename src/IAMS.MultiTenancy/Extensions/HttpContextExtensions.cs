using Microsoft.AspNetCore.Http;
using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Extensions
{
    public static class HttpContextExtensions
    {
        public static Tenant GetCurrentTenant(this HttpContext context)
        {
            return context.Items["CurrentTenant"] as Tenant;
        }

        public static int? GetCurrentTenantId(this HttpContext context)
        {
            return context.Items["TenantId"] as int?;
        }

        public static string GetCurrentTenantIdentifier(this HttpContext context)
        {
            return context.Items["TenantIdentifier"] as string;
        }
    }
}