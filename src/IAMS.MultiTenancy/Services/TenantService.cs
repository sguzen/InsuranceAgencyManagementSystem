// Services/TenantService.cs - Complete Implementation
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;

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

        public async Task<Tenant?> GetTenantAsync(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return null;
            }

            var cacheKey = $"{CacheKeyPrefix}{identifier.ToLowerInvariant()}";

            if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
            {
                _logger.LogDebug("Tenant cache miss for identifier: {Identifier}", identifier);

                try
                {
                    var tenantEntity = await _masterDbContext.Tenants
                        .AsNoTracking()
                        .Include(t => t.TenantModules)
                        .Include(t => t.TenantSettings)
                        .FirstOrDefaultAsync(t => t.Identifier == identifier && t.IsActive);

                    if (tenantEntity != null)
                    {
                        tenant = MapTenantEntityToTenant(tenantEntity);

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

        public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
        {
            var cacheKey = $"{CacheKeyPrefix}id_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
            {
                try
                {
                    var tenantEntity = await _masterDbContext.Tenants
                        .AsNoTracking()
                        .Include(t => t.TenantModules)
                        .Include(t => t.TenantSettings)
                        .FirstOrDefaultAsync(t => t.Id == tenantId);

                    if (tenantEntity != null)
                    {
                        tenant = MapTenantEntityToTenant(tenantEntity);

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
                    .Include(t => t.TenantModules)
                    .Include(t => t.TenantSettings)
                    .Where(t => t.IsActive)
                    .ToListAsync();

                return tenantEntities.Select(MapTenantEntityToTenant).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active tenants");
                return new List<Tenant>();
            }
        }

        public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request)
        {
            try
            {
                // Check if identifier already exists
                var existingTenant = await _masterDbContext.Tenants
                    .FirstOrDefaultAsync(t => t.Identifier == request.Identifier);

                if (existingTenant != null)
                {
                    throw new InvalidOperationException($"Tenant with identifier '{request.Identifier}' already exists.");
                }

                var tenantEntity = new TenantEntity
                {
                    Name = request.Name,
                    Identifier = request.Identifier,
                    ConnectionString = request.ConnectionString,
                    ContactEmail = request.ContactEmail,
                    ContactPhone = request.ContactPhone,
                    SubscriptionPlan = request.SubscriptionPlan,
                    SubscriptionExpiry = request.SubscriptionExpiry,
                    MaxUsers = request.MaxUsers,
                    MaxStorageBytes = request.MaxStorageBytes,
                    TimeZone = request.TimeZone,
                    Currency = request.Currency,
                    Language = request.Language,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true
                };

                _masterDbContext.Tenants.Add(tenantEntity);
                await _masterDbContext.SaveChangesAsync();

                // Add enabled modules
                if (request.EnabledModules.Any())
                {
                    var moduleEntities = request.EnabledModules.Select(module => new TenantModule
                    {
                        TenantId = tenantEntity.Id,
                        ModuleName = module,
                        IsEnabled = true,
                        CreatedOn = DateTime.UtcNow
                    });

                    _masterDbContext.TenantModules.AddRange(moduleEntities);
                }

                // Add settings
                if (request.Settings.Any())
                {
                    var settingEntities = request.Settings.Select(setting => new TenantSetting
                    {
                        TenantId = tenantEntity.Id,
                        SettingKey = setting.Key,
                        SettingValue = setting.Value?.ToString() ?? string.Empty,
                        SettingType = GetSettingType(setting.Value),
                        CreatedOn = DateTime.UtcNow
                    });

                    _masterDbContext.TenantSettings.AddRange(settingEntities);
                }

                await _masterDbContext.SaveChangesAsync();

                _logger.LogInformation("Created new tenant: {TenantId} - {TenantName}", tenantEntity.Id, tenantEntity.Name);

                // Reload with includes
                var createdTenant = await GetTenantByIdAsync(tenantEntity.Id);
                return createdTenant!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant: {TenantName}", request.Name);
                throw;
            }
        }

        public async Task<Tenant> UpdateTenantAsync(int tenantId, UpdateTenantRequest request)
        {
            try
            {
                var tenantEntity = await _masterDbContext.Tenants.FindAsync(tenantId);
                if (tenantEntity == null)
                {
                    throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
                }

                // Update properties
                tenantEntity.Name = request.Name;
                tenantEntity.ContactEmail = request.ContactEmail;
                tenantEntity.ContactPhone = request.ContactPhone;
                tenantEntity.SubscriptionPlan = request.SubscriptionPlan;
                tenantEntity.SubscriptionExpiry = request.SubscriptionExpiry;
                tenantEntity.MaxUsers = request.MaxUsers;
                tenantEntity.MaxStorageBytes = request.MaxStorageBytes;
                tenantEntity.TimeZone = request.TimeZone;
                tenantEntity.Currency = request.Currency;
                tenantEntity.Language = request.Language;
                tenantEntity.IsActive = request.IsActive;
                tenantEntity.LastUpdated = DateTime.UtcNow;

                await _masterDbContext.SaveChangesAsync();

                // Invalidate cache
                await InvalidateTenantCacheAsync(tenantId);

                _logger.LogInformation("Updated tenant: {TenantId} - {TenantName}", tenantId, request.Name);

                var updatedTenant = await GetTenantByIdAsync(tenantId);
                return updatedTenant!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant: {TenantId}", tenantId);
                throw;
            }
        }

        public async Task DeleteTenantAsync(int tenantId)
        {
            try
            {
                var tenantEntity = await _masterDbContext.Tenants.FindAsync(tenantId);
                if (tenantEntity == null)
                {
                    throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
                }

                // Soft delete - just mark as inactive
                tenantEntity.IsActive = false;
                tenantEntity.LastUpdated = DateTime.UtcNow;

                await _masterDbContext.SaveChangesAsync();
                await InvalidateTenantCacheAsync(tenantId);

                _logger.LogInformation("Deleted (deactivated) tenant: {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant: {TenantId}", tenantId);
                throw;
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

        public Tenant? GetCurrentTenant()
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

        public async Task UpdateTenantModuleAsync(int tenantId, string moduleName, bool isEnabled)
        {
            try
            {
                var tenantModule = await _masterDbContext.TenantModules
                    .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.ModuleName == moduleName);

                if (tenantModule == null)
                {
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
                    tenantModule.IsEnabled = isEnabled;
                    tenantModule.LastUpdated = DateTime.UtcNow;
                }

                await _masterDbContext.SaveChangesAsync();
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
                    tenantSetting.SettingValue = stringValue;
                    tenantSetting.SettingType = settingType;
                    tenantSetting.LastUpdated = DateTime.UtcNow;
                }

                await _masterDbContext.SaveChangesAsync();
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

        public async Task<bool> IsSubscriptionActiveAsync(int tenantId)
        {
            try
            {
                var tenant = await GetTenantByIdAsync(tenantId);
                return tenant?.IsSubscriptionActive() ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subscription status for tenant {TenantId}", tenantId);
                return false;
            }
        }

        public async Task UpdateSubscriptionAsync(int tenantId, string subscriptionPlan, DateTime? expiryDate)
        {
            try
            {
                var tenantEntity = await _masterDbContext.Tenants.FindAsync(tenantId);
                if (tenantEntity == null)
                {
                    throw new InvalidOperationException($"Tenant with ID {tenantId} not found.");
                }

                tenantEntity.SubscriptionPlan = subscriptionPlan;
                tenantEntity.SubscriptionExpiry = expiryDate;
                tenantEntity.LastUpdated = DateTime.UtcNow;

                await _masterDbContext.SaveChangesAsync();
                await InvalidateTenantCacheAsync(tenantId);

                _logger.LogInformation("Updated subscription for tenant {TenantId}: {Plan} until {Expiry}",
                    tenantId, subscriptionPlan, expiryDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription for tenant {TenantId}", tenantId);
                throw;
            }
        }

        public async Task<Tenant?> GetTenantByDomainAsync(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                return null;
            }

            // Extract subdomain from domain (e.g., "tenant.example.com" -> "tenant")
            var parts = domain.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            var subdomain = parts[0];
            return await GetTenantAsync(subdomain);
        }

        public async Task<bool> TenantExistsAsync(string identifier)
        {
            try
            {
                return await _masterDbContext.Tenants
                    .AnyAsync(t => t.Identifier == identifier && t.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tenant exists: {Identifier}", identifier);
                return false;
            }
        }

        private static Tenant MapTenantEntityToTenant(TenantEntity tenantEntity)
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

            // Map modules
            foreach (var module in tenantEntity.TenantModules)
            {
                tenant.EnabledModules[module.ModuleName] = module.IsEnabled;
            }

            // Map settings
            foreach (var setting in tenantEntity.TenantSettings)
            {
                var value = ConvertSettingValue(setting.SettingValue, setting.SettingType);
                tenant.Settings[setting.SettingKey] = value;
            }

            return tenant;
        }

        private static object ConvertSettingValue(string value, string type)
        {
            return type.ToLower() switch
            {
                "int" => int.TryParse(value, out var intVal) ? intVal : 0,
                "bool" => bool.TryParse(value, out var boolVal) && boolVal,
                "decimal" => decimal.TryParse(value, out var decVal) ? decVal : 0m,
                "datetime" => DateTime.TryParse(value, out var dateVal) ? dateVal : DateTime.MinValue,
                "double" => double.TryParse(value, out var doubleVal) ? doubleVal : 0d,
                _ => value
            };
        }

        private static string GetSettingType(object? value)
        {
            return value switch
            {
                int => "int",
                bool => "bool",
                decimal => "decimal",
                DateTime => "datetime",
                double => "double",
                float => "double",
                _ => "string"
            };
        }
    }
}