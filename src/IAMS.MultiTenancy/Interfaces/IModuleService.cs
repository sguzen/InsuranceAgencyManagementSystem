namespace IAMS.MultiTenancy.Interfaces
{
    public interface IModuleService
    {
        bool IsModuleEnabled(string moduleName);
        IEnumerable<string> GetEnabledModules();
        Task<List<string>> GetEnabledModulesAsync(int tenantId);
        Task EnableModuleAsync(int tenantId, string moduleName);
        Task DisableModuleAsync(int tenantId, string moduleName);
        Task<Dictionary<string, bool>> GetAllModulesStatusAsync(int tenantId);
    }
}