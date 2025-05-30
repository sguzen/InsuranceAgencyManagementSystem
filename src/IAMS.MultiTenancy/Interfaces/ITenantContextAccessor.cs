﻿using IAMS.MultiTenancy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.MultiTenancy.Interfaces
{
    public interface ITenantContextAccessor
    {
        /// <summary>
        /// Gets the current tenant context
        /// </summary>
        TenantContext TenantContext { get; set; }

        /// <summary>
        /// Gets the current tenant, returns null if no tenant context is set
        /// </summary>
        Tenant CurrentTenant { get; }

        /// <summary>
        /// Gets the current tenant ID, returns null if no tenant context is set
        /// </summary>
        int? CurrentTenantId { get; }

        /// <summary>
        /// Checks if a tenant context is currently set
        /// </summary>
        bool HasTenantContext { get; }

        /// <summary>
        /// Gets the connection string for the current tenant
        /// </summary>
        string GetConnectionString();

        /// <summary>
        /// Checks if the specified module is enabled for the current tenant
        /// </summary>
        bool IsModuleEnabled(string moduleName);

        /// <summary>
        /// Gets a tenant setting value
        /// </summary>
        T GetTenantSetting<T>(string key, T defaultValue = default);

        /// <summary>
        /// Sets a tenant setting value (only for the current request context)
        /// </summary>
        void SetTenantSetting<T>(string key, T value);

        /// <summary>
        /// Gets a context property value
        /// </summary>
        T GetContextProperty<T>(string key, T defaultValue = default);

        /// <summary>
        /// Sets a context property value
        /// </summary>
        void SetContextProperty<T>(string key, T value);

        /// <summary>
        /// Executes an action with a specific tenant context
        /// </summary>
        Task ExecuteWithTenantAsync(Tenant tenant, Func<Task> action);

        /// <summary>
        /// Executes a function with a specific tenant context
        /// </summary>
        Task<T> ExecuteWithTenantAsync<T>(Tenant tenant, Func<Task<T>> function);
    }
}
