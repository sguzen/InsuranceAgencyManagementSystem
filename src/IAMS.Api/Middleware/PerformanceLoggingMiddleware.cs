using System.Diagnostics;

namespace IAMS.Api.Middleware
{
    /// <summary>
    /// Middleware that tracks and logs performance metrics for each request
    /// </summary>
    public class PerformanceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceLoggingMiddleware> _logger;
        private readonly long _slowRequestThresholdMs;

        public PerformanceLoggingMiddleware(
            RequestDelegate next,
            ILogger<PerformanceLoggingMiddleware> logger,
            IConfiguration configuration)
        {
            _next = next;
            _logger = logger;

            // Get slow request threshold from configuration (default: 3000ms)
            _slowRequestThresholdMs = configuration.GetValue<long>("Logging:SlowRequestThresholdMs", 3000);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var startMemory = GC.GetTotalMemory(false);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var endMemory = GC.GetTotalMemory(false);
                var memoryUsed = endMemory - startMemory;

                LogPerformanceMetrics(
                    context,
                    stopwatch.ElapsedMilliseconds,
                    memoryUsed);
            }
        }

        private void LogPerformanceMetrics(
            HttpContext context,
            long elapsedMs,
            long memoryUsedBytes)
        {
            var method = context.Request.Method;
            var path = context.Request.Path;
            var statusCode = context.Response.StatusCode;
            var tenantId = context.Items["TenantId"]?.ToString() ?? "N/A";

            // Always log slow requests as warnings
            if (elapsedMs >= _slowRequestThresholdMs)
            {
                _logger.LogWarning(
                    "SLOW REQUEST | {Method} {Path} | Duration: {Duration}ms | Status: {StatusCode} | Memory: {MemoryKB}KB | TenantId: {TenantId}",
                    method,
                    path,
                    elapsedMs,
                    statusCode,
                    memoryUsedBytes / 1024,
                    tenantId);
            }
            else
            {
                // Log normal requests at debug level with full metrics
                _logger.LogDebug(
                    "Request Completed | {Method} {Path} | Duration: {Duration}ms | Status: {StatusCode} | Memory: {MemoryKB}KB | TenantId: {TenantId}",
                    method,
                    path,
                    elapsedMs,
                    statusCode,
                    memoryUsedBytes / 1024,
                    tenantId);
            }

            // Log performance metrics at information level for aggregation/monitoring
            _logger.LogInformation(
                "Performance Metrics | Path: {Path} | Duration: {Duration}ms | MemoryKB: {MemoryKB} | StatusCode: {StatusCode}",
                path,
                elapsedMs,
                memoryUsedBytes / 1024,
                statusCode);
        }
    }
}
