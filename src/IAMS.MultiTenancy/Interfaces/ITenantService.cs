using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Interfaces
{
    public interface ITenantService
    {
        // Core CRUD operations
        Task<Tenant?> GetTenantAsync(string identifier);
        Task<Tenant?> GetTenantByIdAsync(int tenantId);
        Task<List<Tenant>> GetAllActiveTenantsAsync();
        Task<Tenant> CreateTenantAsync(CreateTenantRequest request);
        Task<Tenant> UpdateTenantAsync(int tenantId, UpdateTenantRequest request);
        Task DeleteTenantAsync(int tenantId);

        // Cache management
        Task InvalidateTenantCacheAsync(string identifier);
        Task InvalidateTenantCacheAsync(int tenantId);

        // Current tenant operations
        Tenant? GetCurrentTenant();
        int? GetCurrentTenantId();

        // Module management
        Task UpdateTenantModuleAsync(int tenantId, string moduleName, bool isEnabled);
        bool IsModuleEnabledForCurrentTenant(string moduleName);

        // Settings management
        Task UpdateTenantSettingAsync(int tenantId, string settingKey, object value, string settingType = "string");

        // Subscription management
        Task<bool> IsSubscriptionActiveAsync(int tenantId);
        Task UpdateSubscriptionAsync(int tenantId, string subscriptionPlan, DateTime? expiryDate);

        // Domain/Identifier resolution
        Task<Tenant?> GetTenantByDomainAsync(string domain);
        Task<bool> TenantExistsAsync(string identifier);
    }

    // Request DTOs
    public class CreateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Identifier { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
        public int MaxUsers { get; set; } = 10;
        public long MaxStorageBytes { get; set; } = 1073741824; // 1GB
        public string TimeZone { get; set; } = "UTC";
        public string Currency { get; set; } = "USD";
        public string Language { get; set; } = "en";
        public List<string> EnabledModules { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class UpdateTenantRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? SubscriptionPlan { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
        public int MaxUsers { get; set; }
        public long MaxStorageBytes { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public string Currency { get; set; } = "USD";
        public string Language { get; set; } = "en";
        public bool IsActive { get; set; } = true;
    }
}