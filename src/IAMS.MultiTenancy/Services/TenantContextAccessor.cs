using Microsoft.AspNetCore.Http;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Data;
using Microsoft.Extensions.Logging;
using IAMS.MultiTenancy.Models;

namespace IAMS.MultiTenancy.Services
{
    public class TenantContextAccessor : ITenantContextAccessor
    {
        private static readonly AsyncLocal<TenantContextHolder> _tenantContextCurrent = new AsyncLocal<TenantContextHolder>();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantContextAccessor> _logger;

        public TenantContextAccessor(
            IHttpContextAccessor httpContextAccessor,
            ILogger<TenantContextAccessor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public TenantContext TenantContext
        {
            get
            {
                // First try to get from HTTP context (for web requests)
                if (_httpContextAccessor.HttpContext?.Items.TryGetValue("TenantContext", out var httpTenantContext) == true)
                {
                    return httpTenantContext as TenantContext;
                }

                // Fall back to AsyncLocal (for background tasks, etc.)
                return _tenantContextCurrent.Value?.Context;
            }
            set
            {
                // Set in HTTP context if available
                if (_httpContextAccessor.HttpContext != null)
                {
                    _httpContextAccessor.HttpContext.Items["TenantContext"] = value;
                }

                // Also set in AsyncLocal for background operations
                var holder = _tenantContextCurrent.Value;
                if (holder != null)
                {
                    holder.Context = null;
                }

                if (value != null)
                {
                    _tenantContextCurrent.Value = new TenantContextHolder { Context = value };
                }
            }
        }

        public Tenant CurrentTenant => TenantContext?.Tenant;

        public int? CurrentTenantId => CurrentTenant?.Id;

        public bool HasTenantContext => TenantContext != null;

        public string GetConnectionString()
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

            // This only sets the value for the current request context
            // To persist changes, you would need to call a tenant service
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

        private class TenantContextHolder
        {
            public TenantContext Context;
        }
    }
}