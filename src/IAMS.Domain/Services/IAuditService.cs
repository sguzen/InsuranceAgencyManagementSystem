using IAMS.Domain.Enums;

namespace IAMS.Domain.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(int tenantId, string userId, AuditAction action, string entityType, int entityId, string? details = null);
        Task LogActionAsync(int tenantId, string userId, AuditAction action, string entityType, int entityId, object? oldValues, object? newValues);
        Task<IEnumerable<object>> GetAuditTrailAsync(int tenantId, string entityType, int entityId);
        Task<IEnumerable<object>> GetUserActionsAsync(int tenantId, string userId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}