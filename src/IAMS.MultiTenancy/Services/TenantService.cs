// Services/TenantService.cs - Simplified for Single Context
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantContext _context; // Your single DbContext
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantService> _logger;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private const string CacheKeyPrefix = "tenant_";
        private const int CacheExpirationMinutes = 30;

        public TenantService(
            TenantContext context, // Your DbContext with DbSets
            IMemoryCache cache,
            ILogger<TenantService> logger,
            ITenantContextAccessor tenantContextAccessor)
        {
            _context = context;
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
                    // Query your DbContext.Tenants (which is DbSet<TenantEntity>)
                    var tenantEntity = await _context.Tenants
                        .AsNoTracking()
                        .Include(t => t.TenantModules)
                        .Include(t => t.TenantSettings)
                        .FirstOrDefaultAsync(t => t.Identifier == identifier && t.IsActive);

                    if (tenantEntity != null)
                    {
                        // Convert TenantEntity to Tenant model
                        tenant = MapEntityToModel(tenantEntity);

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

            return tenant;
        }

        public async Task<Tenant?> GetTenantByIdAsync(int tenantId)
        {
            var cacheKey = $"{CacheKeyPrefix}id_{tenantId}";

            if (!_cache.TryGetValue(cacheKey, out Tenant? tenant))
            {
                try
                {
                    var tenantEntity = await _context.Tenants
                        .AsNoTracking()
                        .Include(t => t.TenantModules)
                        .Include(t => t.TenantSettings)
                        .FirstOrDefaultAsync(t => t.Id == tenantId);

                    if (tenantEntity != null)
                    {
                        tenant = MapEntityToModel(tenantEntity);

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
                var tenantEntities = await _context.Tenants
                    .AsNoTracking()
                    .Include(t => t.TenantModules)
                    .Include(t => t.TenantSettings)
                    .Where(t => t.IsActive)
                    .ToListAsync();

                return tenantEntities.Select(MapEntityToModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all active tenants");
                return new List<Tenant>();
            }
        }

        // ... other methods remain the same but use _context instead of _tenantDbContext

        // The key mapping method: Convert TenantEntity (from DbSet) to Tenant model
        private static Tenant MapEntityToModel(Entities.TenantEntity entity)
        {
            var tenant = new Tenant
            {
                Id = entity.Id,
                Name = entity.Name,
                Identifier = entity.Identifier,
                ConnectionString = entity.ConnectionString,
                IsActive = entity.IsActive,
                CreatedOn = entity.CreatedOn,
                LastUpdated = entity.LastUpdated,
                SubscriptionPlan = entity.SubscriptionPlan,
                SubscriptionExpiry = entity.SubscriptionExpiry,
                MaxUsers = entity.MaxUsers,
                MaxStorageBytes = entity.MaxStorageBytes,
                ContactEmail = entity.ContactEmail,
                ContactPhone = entity.ContactPhone,
                TimeZone = entity.TimeZone ?? "UTC",
                Currency = entity.Currency ?? "USD",
                Language = entity.Language ?? "en",
                EnabledModules = new Dictionary<string, bool>(),
                Settings = new Dictionary<string, object>()
            };

            // Map TenantModule entities to EnabledModules dictionary
            foreach (var module in entity.TenantModules)
            {
                tenant.EnabledModules[module.ModuleName] = module.IsEnabled;
            }

            // Map TenantSetting entities to Settings dictionary
            foreach (var setting in entity.TenantSettings)
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

        // Implement other required interface methods...
        public Tenant? GetCurrentTenant() => _tenantContextAccessor.CurrentTenant;
        public int? GetCurrentTenantId() => _tenantContextAccessor.CurrentTenantId;
        public bool IsModuleEnabledForCurrentTenant(string moduleName) => _tenantContextAccessor.IsModuleEnabled(moduleName);

        // Add stubs for other interface methods
        public Task<Tenant> CreateTenantAsync(CreateTenantRequest request) => throw new NotImplementedException();
        public Task<Tenant> UpdateTenantAsync(int tenantId, UpdateTenantRequest request) => throw new NotImplementedException();
        public Task DeleteTenantAsync(int tenantId) => throw new NotImplementedException();
        public Task InvalidateTenantCacheAsync(string identifier) => throw new NotImplementedException();
        public Task InvalidateTenantCacheAsync(int tenantId) => throw new NotImplementedException();
        public Task UpdateTenantModuleAsync(int tenantId, string moduleName, bool isEnabled) => throw new NotImplementedException();
        public Task UpdateTenantSettingAsync(int tenantId, string settingKey, object value, string settingType = "string") => throw new NotImplementedException();
        public Task<bool> IsSubscriptionActiveAsync(int tenantId) => throw new NotImplementedException();
        public Task UpdateSubscriptionAsync(int tenantId, string subscriptionPlan, DateTime? expiryDate) => throw new NotImplementedException();
        public Task<Tenant?> GetTenantByDomainAsync(string domain) => throw new NotImplementedException();
        public Task<bool> TenantExistsAsync(string identifier) => throw new NotImplementedException();
    }
}