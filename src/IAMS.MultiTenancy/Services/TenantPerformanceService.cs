using IAMS.MultiTenancy.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace IAMS.MultiTenancy.Services
{
    public interface ITenantPerformanceService
    {
        void RecordTenantOperation(string operation, TimeSpan duration, int tenantId);
        Task<TenantPerformanceStats> GetTenantStatsAsync(int tenantId, TimeSpan period);
    }

    public class TenantPerformanceService : ITenantPerformanceService
    {
        private readonly ILogger<TenantPerformanceService> _logger;
        private readonly Dictionary<string, List<PerformanceEntry>> _performanceData;

        public TenantPerformanceService(ILogger<TenantPerformanceService> logger)
        {
            _logger = logger;
            _performanceData = new Dictionary<string, List<PerformanceEntry>>();
        }

        public void RecordTenantOperation(string operation, TimeSpan duration, int tenantId)
        {
            var key = $"{tenantId}:{operation}";
            var entry = new PerformanceEntry
            {
                Operation = operation,
                Duration = duration,
                TenantId = tenantId,
                Timestamp = DateTime.UtcNow
            };

            lock (_performanceData)
            {
                if (!_performanceData.ContainsKey(key))
                {
                    _performanceData[key] = new List<PerformanceEntry>();
                }

                _performanceData[key].Add(entry);

                // Keep only last 1000 entries per operation
                if (_performanceData[key].Count > 1000)
                {
                    _performanceData[key].RemoveAt(0);
                }
            }

            // Log slow operations
            if (duration.TotalSeconds > 5)
            {
                _logger.LogWarning("Slow operation detected: {Operation} for tenant {TenantId} took {Duration}ms",
                    operation, tenantId, duration.TotalMilliseconds);
            }
        }

        public async Task<TenantPerformanceStats> GetTenantStatsAsync(int tenantId, TimeSpan period)
        {
            var cutoff = DateTime.UtcNow.Subtract(period);
            var stats = new TenantPerformanceStats { TenantId = tenantId };

            lock (_performanceData)
            {
                var tenantEntries = _performanceData
                    .Where(kvp => kvp.Key.StartsWith($"{tenantId}:"))
                    .SelectMany(kvp => kvp.Value)
                    .Where(e => e.Timestamp >= cutoff)
                    .ToList();

                if (tenantEntries.Any())
                {
                    stats.TotalOperations = tenantEntries.Count;
                    stats.AverageResponseTime = TimeSpan.FromMilliseconds(
                        tenantEntries.Average(e => e.Duration.TotalMilliseconds));
                    stats.MaxResponseTime = TimeSpan.FromMilliseconds(
                        tenantEntries.Max(e => e.Duration.TotalMilliseconds));
                    stats.MinResponseTime = TimeSpan.FromMilliseconds(
                        tenantEntries.Min(e => e.Duration.TotalMilliseconds));
                }
            }

            return stats;
        }

        private class PerformanceEntry
        {
            public string Operation { get; set; }
            public TimeSpan Duration { get; set; }
            public int TenantId { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }

    public class TenantPerformanceStats
    {
        public int TenantId { get; set; }
        public int TotalOperations { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public TimeSpan MaxResponseTime { get; set; }
        public TimeSpan MinResponseTime { get; set; }
    }

    // Performance monitoring middleware
    public class TenantPerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITenantPerformanceService _performanceService;
        private readonly ITenantContextAccessor _tenantContextAccessor;

        public TenantPerformanceMiddleware(
            RequestDelegate next,
            ITenantPerformanceService performanceService,
            ITenantContextAccessor tenantContextAccessor)
        {
            _next = next;
            _performanceService = performanceService;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var tenantId = _tenantContextAccessor.CurrentTenantId;
                if (tenantId.HasValue)
                {
                    var operation = $"{context.Request.Method} {context.Request.Path}";
                    _performanceService.RecordTenantOperation(operation, stopwatch.Elapsed, tenantId.Value);
                }
            }
        }
    }
}