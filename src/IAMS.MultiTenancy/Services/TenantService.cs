using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.Domain.Entities;

namespace IAMS.MultiTenancy.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantContext _masterDbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantService> _logger;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private const string CacheKeyPrefix = "tenant_";
        private const int CacheExpirationMinutes = 30;

        public TenantService(
            TenantContext masterDbContext,
            IMemoryCache cache,
            ILogger<TenantService> logger,
            ITenantContextAccessor tenantContextAccessor)
        {
            _masterDbContext = masterDbContext;
            _cache = cache;
            _logger = logger;
            _tenantContextAccessor = tenantContextAccessor;
        }

        public async Task<Tenant> GetTenantAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            var cacheKey = $"{CacheKeyPrefix}{identifier.ToLowerInvariant()}";

            if (!_cache.TryGetValue(cacheKey, out Tenant tenant))
            {
                _logger.LogDebug("Tenant cache miss for identifier: {Identifier}", identifier);

                try
                {
                    // Query the master database using EF
                    var tenantEntity = await _masterDbContext.Tenants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Identifier == identifier && t.IsActive);

                    if (tenantEntity != null)
                    {
                        tenant = await MapTenantEntityToTenant(tenantEntity);

                        // Cache the tenant
                        var cacheOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
                            SlidingExpiration = TimeSpan.FromMinutes(10),
                            Priority = CacheItemPriority.High
                        };

                        _cache.Set(cacheKey, tenant, cacheOptions);
                        _logger.LogDebug("Cached tenant {TenantId} for identifier: {Identifier}", tenant.Id, identifier);
                    }
                    else
                    {
                        _logger.LogWarning("Tenant not found for identifier: {Identifier}", identifier);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving tenant for identifier: {Identifier}", identifier);
                    return null;
                }
            }
            else
            {
                _logger.LogDebug("Tenant cache hit for identifier: {Identifier}", identifier);
            }

            return tenant;
        }

        public async Task<Tenant> GetTenantByIdAsync(int tenantId)
        {
            var cacheKey = $"{CacheKeyPrefix}id_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out Tenant tenant))
            {
                try
                {
                    var tenantEntity = await _masterDbContext.Tenants
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == tenantId);

                    if (tenantEntity != null)
                    {
                        tenant = await MapTenantEntityToTenant(tenantEntity);

                        var cacheOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpirationMinutes),
                            SlidingExpiration = TimeSpan.FromMinutes(10),
                            Priority = CacheItemPriority.High
                        };

                        _cache.Set(cacheKey, tenant, cacheOptions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error retrieving tenant by ID: {TenantId}", tenantId);
                    return null;
                }
            }

            return tenant;
        }

        public async Task<List<Tenant>> GetAllActiveTenantsAsync()
        {
            try
            {
                var tenantEntities = await _masterDbContext.Tenants
                    .AsNoTracking()
                    .Where(t => t.IsActive)
                    .ToListAsync();

                var tenants = new List<Tenant>();
                foreach (var tenantEntity in tenantEntities)
                {
                    var tenant = await MapTenantEntityToTenant(tenantEntity);
                    tenants.Add(tenant);
                }

                return tenants;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active tenants");
                return new List<Tenant>();
            }
        }

        public async Task InvalidateTenantCacheAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return;
            }

            var cacheKey = $"{CacheKeyPrefix}{identifier.ToLowerInvariant()}";
            _cache.Remove(cacheKey);

            // Also try to get the tenant ID and invalidate that cache entry
            try
            {
                var tenantId = await _masterDbContext.Tenants
                    .AsNoTracking()
                    .Where(t => t.Identifier == identifier)
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync();

                if (tenantId > 0)
                {
                    var idCacheKey = $"{CacheKeyPrefix}id_{tenantId}";
                    _cache.Remove(idCacheKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating tenant cache for identifier: {Identifier}", identifier);
            }

            _logger.LogInformation("Invalidated tenant cache for identifier: {Identifier}", identifier);
        }

        public async Task InvalidateTenantCacheAsync(int tenantId)
        {
            var idCacheKey = $"{CacheKeyPrefix}id_{tenantId}";
            _cache.Remove(idCacheKey);

            // Also try to get the identifier and invalidate that cache entry
            try
            {
                var identifier = await _masterDbContext.Tenants
                    .AsNoTracking()
                    .Where(t => t.Id == tenantId)
                    .Select(t => t.Identifier)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(identifier))
                {
                    var identifierCacheKey = $"{CacheKeyPrefix}{identifier.ToLowerInvariant()}";
                    _cache.Remove(identifierCacheKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating tenant cache for ID: {TenantId}", tenantId);
            }

            _logger.LogInformation("Invalidated tenant cache for ID: {TenantId}", tenantId);
        }

        public Tenant GetCurrentTenant()
        {
            return _tenantContextAccessor.CurrentTenant;
        }

        public int? GetCurrentTenantId()
        {
            return _tenantContextAccessor.CurrentTenantId;
        }

        public bool IsModuleEnabledForCurrentTenant(string moduleName)
        {
            return _tenantContextAccessor.IsModuleEnabled(moduleName);
        }

        private async Task<Tenant> MapTenantEntityToTenant(ITenantEntity tenantEntity)
        {
            var tenant = new Tenant
            {
                Id = tenantEntity.Id,
                Name = tenantEntity.Name,
                Identifier = tenantEntity.Identifier,
                ConnectionString = tenantEntity.ConnectionString,
                IsActive = tenantEntity.IsActive,
                CreatedOn = tenantEntity.CreatedOn,
                LastUpdated = tenantEntity.LastUpdated,
                SubscriptionPlan = tenantEntity.SubscriptionPlan,
                SubscriptionExpiry = tenantEntity.SubscriptionExpiry,
                MaxUsers = tenantEntity.MaxUsers,
                MaxStorageBytes = tenantEntity.MaxStorageBytes,
                ContactEmail = tenantEntity.ContactEmail,
                ContactPhone = tenantEntity.ContactPhone,
                TimeZone = tenantEntity.TimeZone,
                Currency = tenantEntity.Currency,
                Language = tenantEntity.Language,
                EnabledModules = new Dictionary<string, bool>(),
                Settings = new Dictionary<string, object>()
            };

            // Load enabled modules using EF
            await LoadTenantModulesAsync(tenant);

            // Load tenant settings using EF
            await LoadTenantSettingsAsync(tenant);

            return tenant;
        }

        private async Task LoadTenantModulesAsync(Tenant tenant)
        {
            try
            {
                var modules = await _masterDbContext.TenantModules
                    .AsNoTracking()
                    .Where(tm => tm.TenantId == tenant.Id)
                    .ToListAsync();

                tenant.EnabledModules.Clear();
                foreach (var module in modules)
                {
                    tenant.EnabledModules[module.ModuleName] = module.IsEnabled;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading modules for tenant {TenantId}", tenant.Id);
            }
        }

        private async Task LoadTenantSettingsAsync(Tenant tenant)
        {
            try
            {
                var settings = await _masterDbContext.TenantSettings
                    .AsNoTracking()
                    .Where(ts => ts.TenantId == tenant.Id)
                    .ToListAsync();

                tenant.Settings.Clear();
                foreach (var setting in settings)
                {
                    // Convert the setting value based on its type
                    object value = setting.SettingType switch
                    {
                        "int" => int.Parse(setting.SettingValue),
                        "bool" => bool.Parse(setting.SettingValue),
                        "decimal" => decimal.Parse(setting.SettingValue),
                        "datetime" => DateTime.Parse(setting.SettingValue),
                        _ => setting.SettingValue // Default to string
                    };

                    tenant.Settings[setting.SettingKey] = value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings for tenant {TenantId}", tenant.Id);
            }
        }

        public async Task UpdateTenantModuleAsync(int tenantId, string moduleName, bool isEnabled)
        {
            try
            {
                var tenantModule = await _masterDbContext.TenantModules
                    .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.ModuleName == moduleName);

                if (tenantModule == null)
                {
                    // Create new module entry
                    tenantModule = new TenantModule
                    {
                        TenantId = tenantId,
                        ModuleName = moduleName,
                        IsEnabled = isEnabled,
                        CreatedOn = DateTime.UtcNow
                    };
                    _masterDbContext.TenantModules.Add(tenantModule);
                }
                else
                {
                    // Update existing module
                    tenantModule.IsEnabled = isEnabled;
                    tenantModule.LastUpdated = DateTime.UtcNow;
                }

                await _masterDbContext.SaveChangesAsync();

                // Invalidate cache
                await InvalidateTenantCacheAsync(tenantId);

                _logger.LogInformation("Updated module {ModuleName} for tenant {TenantId} to {IsEnabled}",
                    moduleName, tenantId, isEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating module {ModuleName} for tenant {TenantId}",
                    moduleName, tenantId);
                throw;
            }
        }

        public async Task UpdateTenantSettingAsync(int tenantId, string settingKey, object value, string settingType = "string")
        {
            try
            {
                var tenantSetting = await _masterDbContext.TenantSettings
                    .FirstOrDefaultAsync(ts => ts.TenantId == tenantId && ts.SettingKey == settingKey);

                var stringValue = value?.ToString() ?? string.Empty;

                if (tenantSetting == null)
                {
                    // Create new setting entry
                    tenantSetting = new TenantSetting
                    {
                        TenantId = tenantId,
                        SettingKey = settingKey,
                        SettingValue = stringValue,
                        SettingType = settingType,
                        CreatedOn = DateTime.UtcNow
                    };
                    _masterDbContext.TenantSettings.Add(tenantSetting);
                }
                else
                {
                    // Update existing setting
                    tenantSetting.SettingValue = stringValue;
                    tenantSetting.SettingType = settingType;
                    tenantSetting.LastUpdated = DateTime.UtcNow;
                }

                await _masterDbContext.SaveChangesAsync();

                // Invalidate cache
                await InvalidateTenantCacheAsync(tenantId);

                _logger.LogInformation("Updated setting {SettingKey} for tenant {TenantId}",
                    settingKey, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting {SettingKey} for tenant {TenantId}",
                    settingKey, tenantId);
                throw;
            }
        }
    }
}