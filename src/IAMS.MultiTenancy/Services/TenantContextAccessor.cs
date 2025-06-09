using Microsoft.AspNetCore.Http;
using IAMS.MultiTenancy.Interfaces;
using IAMS.MultiTenancy.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IAMS.MultiTenancy.Services
{
    public class TenantContextAccessor : ITenantContextAccessor
    {
        private static readonly AsyncLocal<TenantContext> _tenantContext = new AsyncLocal<TenantContext>();
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
                // First try HTTP context (for web requests)
                if (_httpContextAccessor.HttpContext?.Items != null &&
                    _httpContextAccessor.HttpContext.Items.TryGetValue("TenantContext", out var httpContext))
                {
                    return httpContext as TenantContext;
                }

                // Fall back to AsyncLocal (for background tasks)
                return _tenantContext.Value;
            }
            set
            {
                // Set in HTTP context if available
                if (_httpContextAccessor.HttpContext?.Items != null)
                {
                    _httpContextAccessor.HttpContext.Items["TenantContext"] = value;
                }

                // Always set in AsyncLocal for consistency
                _tenantContext.Value = value;
            }
        }

        public Tenant CurrentTenant => TenantContext?.Tenant;

        public int? CurrentTenantId => TenantContext?.TenantId;

        public bool HasTenantContext => TenantContext != null;

        public string GetConnectionString()
        {
            var context = TenantContext;
            if (context == null)
            {
                _logger.LogWarning("Attempted to get connection string but no tenant context is set");
                return null;
            }

            return context.DatabaseConnection;
        }

        public bool IsModuleEnabled(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
                return false;

            var context = TenantContext;
            if (context == null)
            {
                _logger.LogWarning("Attempted to check module '{ModuleName}' but no tenant context is set", moduleName);
                return false;
            }

            return context.IsModuleEnabled(moduleName);
        }

        public T GetTenantSetting<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            var context = TenantContext;
            if (context == null)
            {
                _logger.LogWarning("Attempted to get tenant setting '{Key}' but no tenant context is set", key);
                return defaultValue;
            }

            return context.GetTenantSetting(key, defaultValue);
        }

        public T GetRequestProperty<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            var context = TenantContext;
            return context.GetRequestProperty(key, defaultValue) ?? defaultValue;
        }

        public void SetRequestProperty<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var context = TenantContext;
            if (context == null)
            {
                _logger.LogWarning("Attempted to set request property '{Key}' but no tenant context is set", key);
                return;
            }

            context.SetRequestProperty(key, value);
        }

        public async Task ExecuteWithTenantAsync(Tenant tenant, Func<Task> action)
        {
            if (tenant == null)
                throw new ArgumentNullException(nameof(tenant));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

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
                throw new ArgumentNullException(nameof(tenant));
            if (function == null)
                throw new ArgumentNullException(nameof(function));

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
    }
}