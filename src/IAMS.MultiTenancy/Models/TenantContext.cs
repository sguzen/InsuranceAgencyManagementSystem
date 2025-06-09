using IAMS.MultiTenancy.Models;
using System;
using System.Collections.Generic;

namespace IAMS.MultiTenancy.Models
{
    /// <summary>
    /// Represents the current tenant context for a request/operation.
    /// This holds the active tenant and any request-specific data.
    /// </summary>
    public class TenantContext
    {
        private readonly Dictionary<string, object> _requestProperties;

        public TenantContext(Tenant tenant)
        {
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            _requestProperties = new Dictionary<string, object>();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// The current active tenant
        /// </summary>
        public Tenant Tenant { get; }

        /// <summary>
        /// When this context was created
        /// </summary>
        public DateTime CreatedAt { get; }

        /// <summary>
        /// Quick access to tenant ID
        /// </summary>
        public int TenantId => Tenant.Id;

        /// <summary>
        /// Quick access to tenant identifier
        /// </summary>
        public string TenantIdentifier => Tenant.Identifier;

        /// <summary>
        /// Quick access to tenant database connection
        /// </summary>
        public string DatabaseConnection => Tenant.ConnectionString;

        /// <summary>
        /// Check if a module is enabled for this tenant
        /// </summary>
        public bool IsModuleEnabled(string moduleName)
        {
            return Tenant.IsModuleEnabled(moduleName);
        }

        /// <summary>
        /// Get a tenant setting
        /// </summary>
        public T GetTenantSetting<T>(string key, T defaultValue = default)
        {
            return Tenant.GetSetting(key, defaultValue);
        }

        /// <summary>
        /// Store temporary data for this request/operation
        /// Example: User preferences, request-specific flags, etc.
        /// </summary>
        public void SetRequestProperty<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            _requestProperties[key] = value;
        }

        /// <summary>
        /// Get temporary data for this request/operation
        /// </summary>
        public T GetRequestProperty<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            if (_requestProperties.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;

            return defaultValue;
        }

        /// <summary>
        /// Check if a request property exists
        /// </summary>
        public bool HasRequestProperty(string key)
        {
            return !string.IsNullOrEmpty(key) && _requestProperties.ContainsKey(key);
        }

        /// <summary>
        /// Remove a request property
        /// </summary>
        public void RemoveRequestProperty(string key)
        {
            if (!string.IsNullOrEmpty(key))
                _requestProperties.Remove(key);
        }

        /// <summary>
        /// Clear all request properties
        /// </summary>
        public void ClearRequestProperties()
        {
            _requestProperties.Clear();
        }

        /// <summary>
        /// Get all request property keys
        /// </summary>
        public IEnumerable<string> GetRequestPropertyKeys()
        {
            return _requestProperties.Keys;
        }

        /// <summary>
        /// Check if tenant subscription is active
        /// </summary>
        public bool IsSubscriptionActive => Tenant.IsSubscriptionActive();

        /// <summary>
        /// Check if tenant is active
        /// </summary>
        public bool IsActive => Tenant.IsActive;

        /// <summary>
        /// Get tenant's timezone
        /// </summary>
        public string TimeZone => Tenant.TimeZone;

        /// <summary>
        /// Get tenant's currency
        /// </summary>
        public string Currency => Tenant.Currency;

        /// <summary>
        /// Get tenant's language
        /// </summary>
        public string Language => Tenant.Language;

        /// <summary>
        /// Create a summary of this context for logging
        /// </summary>
        public override string ToString()
        {
            return $"TenantContext[Id={TenantId}, Identifier={TenantIdentifier}, Active={IsActive}]";
        }
    }
}
