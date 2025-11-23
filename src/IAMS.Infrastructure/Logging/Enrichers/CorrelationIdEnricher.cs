using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace IAMS.Infrastructure.Logging.Enrichers
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public const string CorrelationIdHeaderName = "X-Correlation-ID";
        public const string CorrelationIdItemKey = "CorrelationId";

        public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            if (httpContext.Items.TryGetValue(CorrelationIdItemKey, out var correlationId) && correlationId != null)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
            }
        }
    }
}
