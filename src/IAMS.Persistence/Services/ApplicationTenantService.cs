using IAMS.Shared.Interfaces;
using IAMS.Domain.Entities;
using IAMS.MultiTenancy.Interfaces;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace IAMS.Persistence.Services
{
    /// <summary>
    /// Implementation of Application layer's ITenantService interface.
    /// Manages tenant-specific settings stored in EACH TENANT'S OWN DATABASE.
    /// Each tenant has their own ApplicationDbContext with a TenantSettings table.
    /// </summary>
    public class ApplicationTenantService : IAMS.Shared.Interfaces.ITenantService
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<ApplicationTenantService> _logger;

        public ApplicationTenantService(
            ITenantContextAccessor tenantContextAccessor,
            ApplicationDbContext applicationDbContext,
            ILogger<ApplicationTenantService> logger)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        public int? GetCurrentTenantId()
        {
            return _tenantContextAccessor.CurrentTenantId;
        }

        public async Task<bool> TenantExistsAsync()
        {
            var tenantId = GetCurrentTenantId();
            if (tenantId == null)
                return false;

            // Check if the ApplicationDbContext is accessible (indicates tenant database exists)
            try
            {
                return await _applicationDbContext.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsModuleEnabledAsync(string moduleName)
        {
            var tenant = _tenantContextAccessor.CurrentTenant;
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

                // Get the setting from the tenant's own database
                var setting = await _applicationDbContext.TenantSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SettingKey == settingKey);

                if (setting == null)
                {
                    _logger.LogDebug("Setting {SettingKey} not found for tenant {TenantId}", settingKey, tenantId);
                    return default;
                }

                // Deserialize the JSON value
                try
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(setting.SettingValue);
                    return deserializedValue;
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize setting {SettingKey} as {Type}", settingKey, typeof(T).Name);
                    return default;
                }
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

                // Find existing setting or create new one
                var setting = await _applicationDbContext.TenantSettings
                    .FirstOrDefaultAsync(s => s.SettingKey == settingKey);

                // Serialize the value to JSON
                var jsonValue = JsonSerializer.Serialize(value, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

                if (setting == null)
                {
                    // Create new setting in tenant's database
                    setting = new TenantSettings
                    {
                        SettingKey = settingKey,
                        SettingValue = jsonValue,
                        Category = InferCategory(settingKey),
                        Description = $"Setting for {settingKey}",
                        IsSystemSetting = false,
                        LastModified = DateTime.UtcNow
                    };

                    await _applicationDbContext.TenantSettings.AddAsync(setting);
                }
                else
                {
                    // Update existing setting
                    setting.SettingValue = jsonValue;
                    setting.LastModified = DateTime.UtcNow;
                    _applicationDbContext.TenantSettings.Update(setting);
                }

                await _applicationDbContext.SaveChangesAsync();

                _logger.LogInformation("Updated tenant setting {SettingKey} for tenant {TenantId} in tenant's own database",
                    settingKey, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting tenant setting: {SettingKey}", settingKey);
                throw;
            }
        }

        /// <summary>
        /// Infers the category from the setting key
        /// </summary>
        private string InferCategory(string settingKey)
        {
            return settingKey.ToLowerInvariant() switch
            {
                var key when key.Contains("smtp") || key.Contains("email") || key.Contains("mail") => "Email",
                var key when key.Contains("currency") || key.Contains("date") || key.Contains("time") || key.Contains("language") => "Regional",
                var key when key.Contains("company") => "Company",
                var key when key.Contains("integration") || key.Contains("api") => "Integration",
                "general" => "General",
                _ => "General"
            };
        }
    }
}
