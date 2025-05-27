using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.MultiTenancy.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Identifier { get; set; } // Subdomain or unique code
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Module and feature management
        public Dictionary<string, bool> EnabledModules { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        // Subscription and billing info
        public string SubscriptionPlan { get; set; }
        public DateTime? SubscriptionExpiry { get; set; }
        public int MaxUsers { get; set; } = 10;
        public long MaxStorageBytes { get; set; } = 1024 * 1024 * 1024; // 1GB default

        // Contact information
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string TimeZone { get; set; } = "UTC";
        public string Currency { get; set; } = "USD";
        public string Language { get; set; } = "en";

        // Helper methods
        public bool IsModuleEnabled(string moduleName)
        {
            return EnabledModules.TryGetValue(moduleName, out bool isEnabled) && isEnabled;
        }

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            if (Settings.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            Settings[key] = value;
        }

        public bool IsSubscriptionActive()
        {
            return IsActive &&
                   (SubscriptionExpiry == null || SubscriptionExpiry > DateTime.UtcNow);
        }
    }

}
