using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Services
{
    public class ModuleService : IModuleService
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly TenantDbContext _tenantdbContext;
        private readonly ILogger<ModuleService> _logger;

        public ModuleService(
            ITenantContextAccessor tenantContextAccessor,
            TenantDbContext tenantDbContext,
            ILogger<ModuleService> logger)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _tenantdbContext = tenantDbContext;
            _logger = logger;
        }

        public bool IsModuleEnabled(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                return false;
            }

            try
            {
                var tenant = _tenantContextAccessor.CurrentTenant;
                if (tenant == null)
                {
                    _logger.LogWarning("No tenant context available when checking module: {ModuleName}", moduleName);
                    return false;
                }

                return tenant.IsModuleEnabled(moduleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if module {ModuleName} is enabled", moduleName);
                return false;
            }
        }

        public IEnumerable<string> GetEnabledModules()
        {
            try
            {
                var tenant = _tenantContextAccessor.CurrentTenant;
                if (tenant == null)
                {
                    _logger.LogWarning("No tenant context available when getting enabled modules");
                    return Enumerable.Empty<string>();
                }

                return tenant.EnabledModules
                    .Where(m => m.Value)
                    .Select(m => m.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enabled modules");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<List<string>> GetEnabledModulesAsync(int tenantId)
        {
            try
            {
                var enabledModules = await _tenantdbContext.TenantModules
                    .Where(tm => tm.TenantId == tenantId && tm.IsEnabled)
                    .Select(tm => tm.ModuleName)
                    .ToListAsync();

                return enabledModules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enabled modules for tenant {TenantId}", tenantId);
                return new List<string>();
            }
        }

        public async Task EnableModuleAsync(int tenantId, string moduleName)
        {
            await SetModuleStatusAsync(tenantId, moduleName, true);
        }

        public async Task DisableModuleAsync(int tenantId, string moduleName)
        {
            await SetModuleStatusAsync(tenantId, moduleName, false);
        }

        public async Task<Dictionary<string, bool>> GetAllModulesStatusAsync(int tenantId)
        {
            try
            {
                var moduleStatuses = await _tenantdbContext.TenantModules
                    .Where(tm => tm.TenantId == tenantId)
                    .ToDictionaryAsync(tm => tm.ModuleName, tm => tm.IsEnabled);

                // Add default modules that might not be in database yet
                var defaultModules = GetDefaultModules();
                foreach (var defaultModule in defaultModules)
                {
                    if (!moduleStatuses.ContainsKey(defaultModule))
                    {
                        moduleStatuses[defaultModule] = false; // Default to disabled
                    }
                }

                return moduleStatuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all module statuses for tenant {TenantId}", tenantId);
                return new Dictionary<string, bool>();
            }
        }

        private async Task SetModuleStatusAsync(int tenantId, string moduleName, bool isEnabled)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                throw new ArgumentException("Module name cannot be null or empty", nameof(moduleName));
            }

            try
            {
                var tenantModule = await _tenantdbContext.TenantModules
                    .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.ModuleName == moduleName);

                if (tenantModule == null)
                {
                    // Create new module entry
                    tenantModule = new Entities.TenantModule
                    {
                        TenantId = tenantId,
                        ModuleName = moduleName,
                        IsEnabled = isEnabled,
                        CreatedOn = DateTime.UtcNow
                    };
                    _tenantdbContext.TenantModules.Add(tenantModule);
                }
                else
                {
                    // Update existing module
                    tenantModule.IsEnabled = isEnabled;
                    tenantModule.LastUpdated = DateTime.UtcNow;
                }

                await _tenantdbContext.SaveChangesAsync();

                var action = isEnabled ? "enabled" : "disabled";
                _logger.LogInformation("Module {ModuleName} {Action} for tenant {TenantId}",
                    moduleName, action, tenantId);
            }
            catch (Exception ex)
            {
                var action = isEnabled ? "enabling" : "disabling";
                _logger.LogError(ex, "Error {Action} module {ModuleName} for tenant {TenantId}",
                    action, moduleName, tenantId);
                throw;
            }
        }

        private static List<string> GetDefaultModules()
        {
            return new List<string>
            {
                "Policy Management",
                "Customer Management",
                "Reporting",
                "User Management",
                "Integration",
                "Accounting",
                "Dashboard",
                "Claims Management",
                "Commission Tracking",
                "Document Management",
                "Notifications",
                "Audit Log"
            };
        }
    }
}