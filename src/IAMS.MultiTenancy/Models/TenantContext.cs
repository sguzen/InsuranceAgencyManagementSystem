using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.MultiTenancy.Models
{
    public class TenantContext
        {
            public Tenant Tenant { get; }
            public Dictionary<string, object> Properties { get; }

            public TenantContext(Tenant tenant)
            {
                Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
                Properties = new Dictionary<string, object>();
            }

            public T GetProperty<T>(string key, T defaultValue = default)
            {
                if (Properties.TryGetValue(key, out var value) && value is T typedValue)
                {
                    return typedValue;
                }
                return defaultValue;
            }

            public void SetProperty<T>(string key, T value)
            {
                Properties[key] = value;
            }

            public bool HasProperty(string key)
            {
                return Properties.ContainsKey(key);
            }

            public void RemoveProperty(string key)
            {
                Properties.Remove(key);
            }
        }
    }
