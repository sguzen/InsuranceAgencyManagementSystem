// IAMS.Application/Interfaces/ICurrentTenantService.cs
namespace IAMS.Application.Interfaces
{
    public interface ICurrentTenantService
    {
        /// <summary>
        /// Gets the current tenant ID
        /// </summary>
        int? TenantId { get; }

        /// <summary>
        /// Gets the current tenant name
        /// </summary>
        string? TenantName { get; }

        /// <summary>
        /// Checks if a tenant context is currently available
        /// </summary>
        bool HasTenant { get; }

        /// <summary>
        /// Checks if the specified module is enabled for the current tenant
        /// </summary>
        bool IsModuleEnabled(string moduleName);

        /// <summary>
        /// Gets a tenant setting value
        /// </summary>
        T GetSetting<T>(string key, T defaultValue = default);
    }
}