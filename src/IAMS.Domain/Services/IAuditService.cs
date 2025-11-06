using IAMS.Domain.Enums;

namespace IAMS.Domain.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, AuditAction action, string entityType, int entityId, string? details = null);
        Task LogActionAsync(string userId, AuditAction action, string entityType, int entityId, object? oldValues, object? newValues);
        Task<IEnumerable<object>> GetAuditTrailAsync(string entityType, int entityId);
        Task<IEnumerable<object>> GetUserActionsAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}