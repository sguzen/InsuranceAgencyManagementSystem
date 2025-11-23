namespace IAMS.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for audit logging of business-critical operations
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Log an audit event
        /// </summary>
        Task LogAsync(AuditLog auditLog);

        /// <summary>
        /// Log an audit event for a specific action
        /// </summary>
        Task LogActionAsync(
            string action,
            string entityType,
            string? entityId = null,
            object? oldValues = null,
            object? newValues = null,
            string? description = null);

        /// <summary>
        /// Log a successful authentication
        /// </summary>
        Task LogAuthenticationAsync(string userId, string username, bool success, string? reason = null);

        /// <summary>
        /// Log an authorization decision
        /// </summary>
        Task LogAuthorizationAsync(string userId, string resource, string action, bool granted, string? reason = null);

        /// <summary>
        /// Log data access (for sensitive data)
        /// </summary>
        Task LogDataAccessAsync(string entityType, string entityId, string action);

        /// <summary>
        /// Log configuration changes
        /// </summary>
        Task LogConfigurationChangeAsync(string configKey, object? oldValue, object? newValue, string? reason = null);
    }

    /// <summary>
    /// Audit log entry model
    /// </summary>
    public class AuditLog
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? TenantId { get; set; }
        public string? TenantIdentifier { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public object? OldValues { get; set; }
        public object? NewValues { get; set; }
        public string? Description { get; set; }
        public string? CorrelationId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}
