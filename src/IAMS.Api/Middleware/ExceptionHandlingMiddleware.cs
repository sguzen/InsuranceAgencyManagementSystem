using System.Net;
using System.Text.Json;
using IAMS.Application.Exceptions;

namespace IAMS.Api.Middleware
{
    /// <summary>
    /// Centralized exception handling middleware that catches all unhandled exceptions
    /// and returns appropriate HTTP responses while logging the errors
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errorCode) = GetErrorDetails(exception);

            // Log the exception with full details
            _logger.LogError(
                exception,
                "Unhandled exception occurred | StatusCode: {StatusCode} | ErrorCode: {ErrorCode} | Path: {Path} | Method: {Method}",
                statusCode,
                errorCode,
                context.Request.Path,
                context.Request.Method);

            // Build error response
            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                ErrorCode = errorCode,
                TraceId = context.TraceIdentifier,
                CorrelationId = context.Items["CorrelationId"]?.ToString(),
                Timestamp = DateTime.UtcNow
            };

            // Include detailed error information in development
            if (_environment.IsDevelopment())
            {
                response.Details = exception.Message;
                response.StackTrace = exception.StackTrace;
                response.InnerException = exception.InnerException?.Message;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private (int statusCode, string message, string errorCode) GetErrorDetails(Exception exception)
        {
            return exception switch
            {
                // Application-specific exceptions
                NotFoundException => ((int)HttpStatusCode.NotFound, exception.Message, "RESOURCE_NOT_FOUND"),
                ValidationException => ((int)HttpStatusCode.BadRequest, exception.Message, "VALIDATION_ERROR"),
                UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized access", "UNAUTHORIZED"),
                TenantException => ((int)HttpStatusCode.BadRequest, exception.Message, "TENANT_ERROR"),

                // Business rule violations (more specific exceptions must come before their base types)
                InvalidOperationException => ((int)HttpStatusCode.BadRequest, exception.Message, "INVALID_OPERATION"),
                ArgumentNullException => ((int)HttpStatusCode.BadRequest, "Required parameter is missing", "MISSING_PARAMETER"),
                ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message, "INVALID_ARGUMENT"),

                // Default to 500 Internal Server Error
                _ => ((int)HttpStatusCode.InternalServerError,
                      _environment.IsDevelopment() ? exception.Message : "An internal server error occurred",
                      "INTERNAL_SERVER_ERROR")
            };
        }

        private class ErrorResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; } = string.Empty;
            public string ErrorCode { get; set; } = string.Empty;
            public string? TraceId { get; set; }
            public string? CorrelationId { get; set; }
            public DateTime Timestamp { get; set; }
            public string? Details { get; set; }
            public string? StackTrace { get; set; }
            public string? InnerException { get; set; }
        }
    }
}
