// Services/TenantContextAccessor.cs - Simple Implementation
using Microsoft.AspNetCore.Http;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using Microsoft.Extensions.Logging;

namespace IAMS.MultiTenancy.Services
{
    public class TenantContextAccessor : ITenantContextAccessor
    {
        private static readonly AsyncLocal<TenantHolder> _currentTenant = new AsyncLocal<TenantHolder>();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantContextAccessor> _logger;

        public TenantContextAccessor(
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantContextAccessor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public TenantContext? TenantContext
        {
            get
            {
                // Try to get from HTTP context first
                if (_httpContextAccessor.HttpContext?.Items.TryGetValue("TenantContext", out var context) == true)
                {
                    return context as TenantContext;
                }

                // Fall back to AsyncLocal
                var holder = _currentTenant.Value;
                if (holder?.Tenant != null)
                {
                    return new TenantContext(holder.Tenant);
                }

                return null;
            }
            set
            {
                // Set in HTTP context if available
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items["TenantContext"] = value;
                }

                // Also set in AsyncLocal
                if (value?.Tenant != null)
                {
                    _currentTenant.Value = new TenantHolder { Tenant = value.Tenant };
                }
                else
                {
                    _currentTenant.Value = null;
                }
            }
        }

        public Tenant? CurrentTenant
        {
            get
            {
                // Try HTTP context first
                if (_httpContextAccessor.HttpContext?.Items.TryGetValue("CurrentTenant", out var tenant) == true)
                {
                    return tenant as Tenant;
                }

                // Fall back to TenantContext
                return TenantContext?.Tenant;
            }
        }

        public int? CurrentTenantId => CurrentTenant?.Id;

        public bool HasTenantContext => CurrentTenant != null;

        public string? GetConnectionString()
        {
            var tenant = CurrentTenant;
            if (tenant == null)
            {
                _logger.LogWarning("Attempted to get connection string but no tenant context is set");
                return null;
            }

            if (string.IsNullOrEmpty(tenant.ConnectionString))
            {
                _logger.LogError("Tenant {TenantId} has no connection string configured", tenant.Id);
                return null;
            }

            return tenant.ConnectionString;
        }

        public bool IsModuleEnabled(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                return false;
            }

            var tenant = CurrentTenant;
            if (tenant == null)
            {
                _logger.LogWarning("Attempted to check module '{ModuleName}' but no tenant context is set", moduleName);
                return false;
            }

            return tenant.IsModuleEnabled(moduleName);
        }

        public T GetTenantSetting<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            var tenant = CurrentTenant;
            if (tenant == null)
            {
                _logger.LogWarning("Attempted to get tenant setting '{Key}' but no tenant context is set", key);
                return defaultValue;
            }

            return tenant.GetSetting(key, defaultValue);
        }

        public void SetTenantSetting<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var tenant = CurrentTenant;
            if (tenant == null)
            {
                _logger.LogWarning("Attempted to set tenant setting '{Key}' but no tenant context is set", key);
                return;
            }

            tenant.SetSetting(key, value);
        }

        public T GetContextProperty<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }

            var context = TenantContext;
            if (context == null)
            {
                return defaultValue;
            }

            return context.GetProperty(key, defaultValue);
        }

        public void SetContextProperty<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var context = TenantContext;
            if (context == null)
            {
                _logger.LogWarning("Attempted to set context property '{Key}' but no tenant context is set", key);
                return;
            }

            context.SetProperty(key, value);
        }

        public async Task ExecuteWithTenantAsync(Tenant tenant, Func<Task> action)
        {
            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var previousContext = TenantContext;
            try
            {
                TenantContext = new TenantContext(tenant);
                await action();
            }
            finally
            {
                TenantContext = previousContext;
            }
        }

        public async Task<T> ExecuteWithTenantAsync<T>(Tenant tenant, Func<Task<T>> function)
        {
            if (tenant == null)
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (function == null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            var previousContext = TenantContext;
            try
            {
                TenantContext = new TenantContext(tenant);
                return await function();
            }
            finally
            {
                TenantContext = previousContext;
            }
        }

        private class TenantHolder
        {
            public Tenant? Tenant { get; set; }
        }
    }
}