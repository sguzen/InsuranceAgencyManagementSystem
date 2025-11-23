using System.Diagnostics;
using System.Text;
using Microsoft.IO;

namespace IAMS.Api.Middleware
{
    /// <summary>
    /// Middleware that logs HTTP requests and responses with configurable detail levels
    /// </summary>
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        private readonly HashSet<string> _excludedPaths;

        public HttpLoggingMiddleware(
            RequestDelegate next,
            ILogger<HttpLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

            // Paths to exclude from detailed logging
            _excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "/health",
                "/healthz",
                "/ready",
                "/live",
                "/swagger",
                "/favicon.ico"
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip logging for excluded paths
            if (ShouldSkipLogging(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            // Log the request
            await LogRequestAsync(context.Request);

            // Capture response
            var originalBodyStream = context.Response.Body;
            await using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                stopwatch.Stop();

                // Log the response
                await LogResponseAsync(context.Response, stopwatch.ElapsedMilliseconds);
            }
            finally
            {
                // Copy response back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequestAsync(HttpRequest request)
        {
            request.EnableBuffering();

            var requestBody = string.Empty;

            // Only read body for non-GET requests and if content length is reasonable
            if (request.ContentLength.HasValue
                && request.ContentLength > 0
                && request.ContentLength < 10000
                && request.Method != "GET")
            {
                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            _logger.LogInformation(
                "HTTP {Method} {Path}{QueryString} Request | ContentType: {ContentType} | ContentLength: {ContentLength}",
                request.Method,
                request.Path,
                request.QueryString,
                request.ContentType,
                request.ContentLength);

            // Log body for non-GET requests in debug mode
            if (!string.IsNullOrEmpty(requestBody))
            {
                _logger.LogDebug("Request Body: {RequestBody}", requestBody);
            }
        }

        private async Task LogResponseAsync(HttpResponse response, long elapsedMs)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = string.Empty;

            // Only read response body if it's a reasonable size and not a file download
            if (response.ContentLength.HasValue
                && response.ContentLength > 0
                && response.ContentLength < 10000
                && IsTextContentType(response.ContentType))
            {
                using var reader = new StreamReader(response.Body, Encoding.UTF8, leaveOpen: true);
                responseBody = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
            }

            var logLevel = response.StatusCode >= 500 ? LogLevel.Error
                         : response.StatusCode >= 400 ? LogLevel.Warning
                         : LogLevel.Information;

            _logger.Log(
                logLevel,
                "HTTP Response {StatusCode} | Duration: {Duration}ms | ContentType: {ContentType} | ContentLength: {ContentLength}",
                response.StatusCode,
                elapsedMs,
                response.ContentType,
                response.ContentLength);

            // Log response body in debug mode for errors
            if (!string.IsNullOrEmpty(responseBody) && response.StatusCode >= 400)
            {
                _logger.LogDebug("Response Body: {ResponseBody}", responseBody);
            }
        }

        private bool ShouldSkipLogging(PathString path)
        {
            return _excludedPaths.Any(excluded => path.StartsWithSegments(excluded));
        }

        private static bool IsTextContentType(string? contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            return contentType.Contains("text", StringComparison.OrdinalIgnoreCase)
                   || contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
                   || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
        }
    }
}
