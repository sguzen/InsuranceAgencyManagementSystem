using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using IAMS.Infrastructure.Interfaces;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Implementation of audit logging service that logs to structured logs
    /// </summary>
    public class AuditLogger : IAuditLogger
    {
        private readonly ILogger<AuditLogger> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogger(
            ILogger<AuditLogger> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task LogAsync(AuditLog auditLog)
        {
            EnrichAuditLogFromContext(auditLog);

            _logger.LogInformation(
                "AUDIT: {Action} | Entity: {EntityType}/{EntityId} | User: {UserId} | Tenant: {TenantId} | Description: {Description}",
                auditLog.Action,
                auditLog.EntityType,
                auditLog.EntityId ?? "N/A",
                auditLog.UserId ?? "Anonymous",
                auditLog.TenantId ?? "N/A",
                auditLog.Description ?? "No description");

            // Log with full audit log object for structured logging
            _logger.LogInformation("AUDIT_DETAILS: {AuditLog}", JsonSerializer.Serialize(auditLog));

            return Task.CompletedTask;
        }

        public Task LogActionAsync(
            string action,
            string entityType,
            string? entityId = null,
            object? oldValues = null,
            object? newValues = null,
            string? description = null)
        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                Description = description
            };

            return LogAsync(auditLog);
        }

        public Task LogAuthenticationAsync(string userId, string username, bool success, string? reason = null)
        {
            var auditLog = new AuditLog
            {
                Action = success ? "AUTHENTICATION_SUCCESS" : "AUTHENTICATION_FAILED",
                EntityType = "User",
                EntityId = userId,
                Username = username,
                Description = reason ?? (success ? "User authenticated successfully" : "Authentication failed"),
                AdditionalData = new Dictionary<string, object>
                {
                    ["Success"] = success,
                    ["Reason"] = reason ?? string.Empty
                }
            };

            return LogAsync(auditLog);
        }

        public Task LogAuthorizationAsync(string userId, string resource, string action, bool granted, string? reason = null)
        {
            var auditLog = new AuditLog
            {
                Action = granted ? "AUTHORIZATION_GRANTED" : "AUTHORIZATION_DENIED",
                EntityType = "Authorization",
                EntityId = resource,
                UserId = userId,
                Description = $"User {(granted ? "granted" : "denied")} access to {action} on {resource}",
                AdditionalData = new Dictionary<string, object>
                {
                    ["Resource"] = resource,
                    ["RequestedAction"] = action,
                    ["Granted"] = granted,
                    ["Reason"] = reason ?? string.Empty
                }
            };

            return LogAsync(auditLog);
        }

        public Task LogDataAccessAsync(string entityType, string entityId, string action)
        {
            var auditLog = new AuditLog
            {
                Action = $"DATA_ACCESS_{action.ToUpperInvariant()}",
                EntityType = entityType,
                EntityId = entityId,
                Description = $"Accessed {entityType} data (ID: {entityId})"
            };

            return LogAsync(auditLog);
        }

        public Task LogConfigurationChangeAsync(string configKey, object? oldValue, object? newValue, string? reason = null)
        {
            var auditLog = new AuditLog
            {
                Action = "CONFIGURATION_CHANGE",
                EntityType = "Configuration",
                EntityId = configKey,
                OldValues = oldValue,
                NewValues = newValue,
                Description = reason ?? $"Configuration '{configKey}' was changed"
            };

            return LogAsync(auditLog);
        }

        private void EnrichAuditLogFromContext(AuditLog auditLog)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            // Add user information if not already set
            if (string.IsNullOrEmpty(auditLog.UserId) && httpContext.User?.Identity?.IsAuthenticated == true)
            {
                auditLog.UserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                  ?? httpContext.User.FindFirst("sub")?.Value;
                auditLog.Username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value
                                    ?? httpContext.User.FindFirst("name")?.Value
                                    ?? httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            }

            // Add tenant information
            if (string.IsNullOrEmpty(auditLog.TenantId))
            {
                auditLog.TenantId = httpContext.Items["TenantId"]?.ToString();
                auditLog.TenantIdentifier = httpContext.Items["TenantIdentifier"]?.ToString();
            }

            // Add correlation ID
            if (string.IsNullOrEmpty(auditLog.CorrelationId))
            {
                auditLog.CorrelationId = httpContext.Items["CorrelationId"]?.ToString();
            }

            // Add IP address
            if (string.IsNullOrEmpty(auditLog.IpAddress))
            {
                auditLog.IpAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            }

            // Add user agent
            if (string.IsNullOrEmpty(auditLog.UserAgent))
            {
                auditLog.UserAgent = httpContext.Request.Headers.UserAgent.ToString();
            }
        }
    }
}
