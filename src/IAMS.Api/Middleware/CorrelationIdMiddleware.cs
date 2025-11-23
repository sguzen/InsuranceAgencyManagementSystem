using Serilog.Context;

namespace IAMS.Api.Middleware
{
    /// <summary>
    /// Middleware that ensures every request has a unique correlation ID for tracing
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";
        private const string CorrelationIdItemKey = "CorrelationId";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get or generate correlation ID
            string correlationId = GetOrGenerateCorrelationId(context);

            // Store in HttpContext.Items for access throughout the request pipeline
            context.Items[CorrelationIdItemKey] = correlationId;

            // Add to response headers for client tracking
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
                {
                    context.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);
                }
                return Task.CompletedTask;
            });

            // Push to Serilog LogContext for automatic inclusion in all logs during this request
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetOrGenerateCorrelationId(HttpContext context)
        {
            // Check if client provided a correlation ID
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
                && !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.ToString();
            }

            // Generate new correlation ID
            return Guid.NewGuid().ToString("N");
        }
    }
}
