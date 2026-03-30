using IAMS.MultiTenancy.Data;
using IAMS.MultiTenancy.Entities;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IAMS.MultiTenancy.Services
{
    /// <summary>
    /// Service for managing tenant metadata stored in the master database.
    /// Reads tenant connection strings from the Tenants table instead of appsettings.
    /// </summary>
    public class TenantService : IAMS.MultiTenancy.Interfaces.ITenantService
    {
        private readonly TenantDbContext _context;
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantService> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        private const string TenantCacheKeyPrefix = "Tenant_";
        private const string AllTenantsCacheKey = "AllActiveTenants";

        public TenantService(
            TenantDbContext context,
            ITenantContextAccessor tenantContextAccessor,
            IMemoryCache cache,
            ILogger<TenantService> logger)
        {
            _context = context;
            _tenantContextAccessor = tenantContextAccessor;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Tenant> GetTenantAsync(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                _logger.LogWarning("GetTenantAsync called with null/empty identifier");
                return null;
            }

            var cacheKey = $"{TenantCacheKeyPrefix}{identifier}";

            if (_cache.TryGetValue(cacheKey, out Tenant cachedTenant))
            {
                _logger.LogDebug("Tenant {Identifier} found in cache", identifier);
                return cachedTenant;
            }

            _logger.LogDebug("Loading tenant {Identifier} from database", identifier);

            var tenantEntity = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.TenantModules)
                .FirstOrDefaultAsync(t => t.Identifier == identifier && !t.IsDeleted);

            if (tenantEntity == null)
            {
                _logger.LogWarning("Tenant {Identifier} not found in database", identifier);
                return null;
            }

            var tenant = MapToTenant(tenantEntity);

            _cache.Set(cacheKey, tenant, _cacheExpiration);
            _logger.LogDebug("Tenant {Identifier} cached", identifier);

            return tenant;
        }

        public async Task<Tenant> GetTenantByIdAsync()
        {
            var tenantId = _tenantContextAccessor.CurrentTenantId;
            if (tenantId == null)
            {
                _logger.LogWarning("GetTenantByIdAsync called but no tenant context");
                return null;
            }

            var cacheKey = $"{TenantCacheKeyPrefix}Id_{tenantId}";

            if (_cache.TryGetValue(cacheKey, out Tenant cachedTenant))
            {
                return cachedTenant;
            }

            var tenantEntity = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.TenantModules)
                .FirstOrDefaultAsync(t => t.Id == tenantId.Value && !t.IsDeleted);

            if (tenantEntity == null)
            {
                return null;
            }

            var tenant = MapToTenant(tenantEntity);
            _cache.Set(cacheKey, tenant, _cacheExpiration);

            return tenant;
        }

        public async Task<List<Tenant>> GetAllActiveTenantsAsync()
        {
            if (_cache.TryGetValue(AllTenantsCacheKey, out List<Tenant> cachedTenants))
            {
                return cachedTenants;
            }

            var tenantEntities = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.TenantModules)
                .Where(t => t.IsActive && !t.IsDeleted)
                .ToListAsync();

            var tenants = tenantEntities.Select(MapToTenant).ToList();

            _cache.Set(AllTenantsCacheKey, tenants, _cacheExpiration);
            _logger.LogDebug("Cached {Count} active tenants", tenants.Count);

            return tenants;
        }

        public Task InvalidateTenantCacheAsync(string identifier)
        {
            var cacheKey = $"{TenantCacheKeyPrefix}{identifier}";
            _cache.Remove(cacheKey);
            _cache.Remove(AllTenantsCacheKey);
            _logger.LogInformation("Invalidated cache for tenant {Identifier}", identifier);
            return Task.CompletedTask;
        }

        public Task InvalidateTenantCacheAsync()
        {
            var tenantId = _tenantContextAccessor.CurrentTenantId;
            if (tenantId != null)
            {
                var cacheKey = $"{TenantCacheKeyPrefix}Id_{tenantId}";
                _cache.Remove(cacheKey);
            }
            _cache.Remove(AllTenantsCacheKey);
            return Task.CompletedTask;
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

        public async Task UpdateTenantModuleAsync(string moduleName, bool isEnabled)
        {
            var tenantId = _tenantContextAccessor.CurrentTenantId;
            if (tenantId == null)
            {
                throw new InvalidOperationException("No tenant context available");
            }

            var module = await _context.TenantModules
                .FirstOrDefaultAsync(m => m.TenantId == tenantId.Value && m.ModuleName == moduleName);

            if (module == null)
            {
                module = new TenantModule
                {
                    TenantId = tenantId.Value,
                    ModuleName = moduleName,
                    IsEnabled = isEnabled,
                    CreatedOn = DateTime.UtcNow
                };
                _context.TenantModules.Add(module);
            }
            else
            {
                module.IsEnabled = isEnabled;
                module.ModifiedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await InvalidateTenantCacheAsync();

            _logger.LogInformation("Updated module {ModuleName} to {IsEnabled} for tenant {TenantId}",
                moduleName, isEnabled, tenantId);
        }

        private static Tenant MapToTenant(TenantEntity entity)
        {
            return new Tenant
            {
                Id = entity.Id,
                Name = entity.Name,
                Identifier = entity.Identifier,
                ConnectionString = entity.ConnectionString,
                IsActive = entity.IsActive,
                SubscriptionPlan = entity.SubscriptionPlan,
                MaxUsers = entity.MaxUsers,
                MaxStorageBytes = entity.MaxStorageBytes,
                ContactEmail = entity.ContactEmail,
                ExternalId = entity.ExternalId,
                TimeZone = entity.TimeZone,
                Currency = entity.Currency,
                Language = entity.Language,
                EnabledModules = entity.TenantModules?
                    .ToDictionary(m => m.ModuleName, m => m.IsEnabled)
                    ?? new Dictionary<string, bool>(),
                Settings = new Dictionary<string, object>()
            };
        }
    }
}
