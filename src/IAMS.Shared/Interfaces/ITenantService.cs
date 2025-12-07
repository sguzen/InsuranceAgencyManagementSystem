namespace IAMS.Shared.Interfaces
{
    public interface ITenantService
    {
        int? GetCurrentTenantId();
        Task<bool> TenantExistsAsync();
        Task<bool> IsModuleEnabledAsync(string moduleName);
        Task<T?> GetTenantSettingAsync<T>(string settingKey);
        Task SetTenantSettingAsync<T>(string settingKey, T value);
    }
}