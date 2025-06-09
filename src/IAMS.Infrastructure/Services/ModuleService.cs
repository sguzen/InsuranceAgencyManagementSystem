using IAMS.Infrastructure.Interfaces;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Infrastructure.Services
{
    // IAMS.Infrastructure/Services/ModuleService.cs
    public class ModuleService : IModuleService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ModuleService> _logger;
        private static readonly Dictionary<string, bool> _moduleCache = new();

        public ModuleService(IConfiguration configuration, ILogger<ModuleService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            LoadModuleConfiguration();
        }

        public async Task<bool> IsModuleEnabledAsync(string moduleName)
        {
            return await Task.FromResult(IsModuleEnabled(moduleName));
        }

        public bool IsModuleEnabled(string moduleName)
        {
            if (_moduleCache.TryGetValue(moduleName.ToLower(), out var isEnabled))
            {
                return isEnabled;
            }

            // Default to false if not configured
            return false;
        }

        public async Task<List<string>> GetEnabledModulesAsync()
        {
            var enabledModules = _moduleCache.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            return await Task.FromResult(enabledModules);
        }

        public async Task<bool> EnableModuleAsync(string moduleName)
        {
            try
            {
                _moduleCache[moduleName.ToLower()] = true;
                _logger.LogInformation("Module {ModuleName} enabled", moduleName);

                // In a real implementation, you would update the database/configuration
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enable module {ModuleName}", moduleName);
                return false;
            }
        }

        public async Task<bool> DisableModuleAsync(string moduleName)
        {
            try
            {
                _moduleCache[moduleName.ToLower()] = false;
                _logger.LogInformation("Module {ModuleName} disabled", moduleName);

                // In a real implementation, you would update the database/configuration
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disable module {ModuleName}", moduleName);
                return false;
            }
        }

        private void LoadModuleConfiguration()
        {
            var modules = _configuration.GetSection("Modules").Get<Dictionary<string, bool>>() ?? new();

            foreach (var module in modules)
            {
                _moduleCache[module.Key.ToLower()] = module.Value;
            }

            // Set default modules for insurance agency
            SetDefaultIfNotConfigured("reporting", true);
            SetDefaultIfNotConfigured("accounting", false); // Premium module
            SetDefaultIfNotConfigured("integration", false); // Premium module
            SetDefaultIfNotConfigured("claims", true);
            SetDefaultIfNotConfigured("policies", true);
            SetDefaultIfNotConfigured("customers", true);
        }

        private void SetDefaultIfNotConfigured(string moduleName, bool defaultValue)
        {
            if (!_moduleCache.ContainsKey(moduleName.ToLower()))
            {
                _moduleCache[moduleName.ToLower()] = defaultValue;
            }
        }
        // unimplemented
        public IEnumerable<string> GetEnabledModules()
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetEnabledModulesAsync(int tenantId)
        {
            throw new NotImplementedException();
        }

        public Task EnableModuleAsync(int tenantId, string moduleName)
        {
            throw new NotImplementedException();
        }

        public Task DisableModuleAsync(int tenantId, string moduleName)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, bool>> GetAllModulesStatusAsync(int tenantId)
        {
            throw new NotImplementedException();
        }
    }

   
}
