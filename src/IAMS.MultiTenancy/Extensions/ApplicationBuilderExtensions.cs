using Microsoft.AspNetCore.Builder;
using IAMS.MultiTenancy.Middleware;

namespace IAMS.MultiTenancy.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMultiTenancy(this IApplicationBuilder app)
        {
            return app.UseMiddleware<TenantMiddleware>();
        }

        public static IApplicationBuilder UseTenantPerformanceMonitoring(this IApplicationBuilder app)
        {
            return app.UseMiddleware<Services.TenantPerformanceMiddleware>();
        }
    }
}