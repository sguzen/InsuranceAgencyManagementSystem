using IAMS.Application.Interfaces;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IAMS.MultiTenancy.Services
{
    /// <summary>
    /// Implementation of Application layer's ITenantService interface
    /// for managing tenant-specific settings stored in the master database
    /// </summary>
    public class ApplicationTenantService : IAMS.Application.Interfaces.ITenantService
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly TenantDbContext _tenantDbContext;
        private readonly ILogger<ApplicationTenantService> _logger;

        public ApplicationTenantService(
            ITenantContextAccessor tenantContextAccessor,
            TenantDbContext tenantDbContext,
            ILogger<ApplicationTenantService> logger)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenantDbContext = tenantDbContext;
            _logger = logger;
        }

        public int? GetCurrentTenantId()
        {
            return _tenantContextAccessor.Current?.Id;
        }

        public async Task<bool> TenantExistsAsync()
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == null)
                return false;

            return await _tenantDbContext.Tenants
                .AnyAsync(t => t.Id == tenantId.Value && t.IsActive);
        }

        public async Task<bool> IsModuleEnabledAsync(string moduleName)
        {
            var tenant = _tenantContextAccessor.Current;
            if (tenant == null)
                return false;

            return tenant.IsModuleEnabled(moduleName);
        }

        public async Task<T?> GetTenantSettingAsync<T>(string settingKey)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                if (tenantId == null)
                {
                    _logger.LogWarning("No tenant context available for getting setting: {SettingKey}", settingKey);
                    return default;
                }

                var tenant = await _tenantDbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == tenantId.Value);

                if (tenant == null)
                {
                    _logger.LogWarning("Tenant not found: {TenantId}", tenantId);
                    return default;
                }

                // Get the setting from the tenant's Settings dictionary
                if (tenant.Settings != null && tenant.Settings.TryGetValue(settingKey, out var settingValue))
                {
                    // If the value is already of type T, return it
                    if (settingValue is T typedValue)
                    {
                        return typedValue;
                    }

                    // Try to deserialize from JSON if it's a string
                    if (settingValue is string jsonString)
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<T>(jsonString);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize setting {SettingKey} as {Type}", settingKey, typeof(T).Name);
                        }
                    }

                    // Try to convert the value
                    try
                    {
                        return (T)Convert.ChangeType(settingValue, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert setting {SettingKey} to {Type}", settingKey, typeof(T).Name);
                    }
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant setting: {SettingKey}", settingKey);
                throw;
            }
        }

        public async Task SetTenantSettingAsync<T>(string settingKey, T value)
        {
            try
            {
                var tenantId = GetCurrentTenantId();
                if (tenantId == null)
                {
                    throw new InvalidOperationException("No tenant context available");
                }

                var tenant = await _tenantDbContext.Tenants
                    .FirstOrDefaultAsync(t => t.Id == tenantId.Value);

                if (tenant == null)
                {
                    throw new InvalidOperationException($"Tenant not found: {tenantId}");
                }

                // Initialize Settings dictionary if null
                tenant.Settings ??= new Dictionary<string, object>();

                // Serialize complex objects to JSON
                if (typeof(T).IsClass && typeof(T) != typeof(string))
                {
                    var jsonValue = JsonSerializer.Serialize(value);
                    tenant.Settings[settingKey] = jsonValue;
                }
                else
                {
                    tenant.Settings[settingKey] = value!;
                }

                tenant.LastUpdated = DateTime.UtcNow;

                await _tenantDbContext.SaveChangesAsync();

                _logger.LogInformation("Updated tenant setting {SettingKey} for tenant {TenantId}", settingKey, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting tenant setting: {SettingKey}", settingKey);
                throw;
            }
        }
    }
}
