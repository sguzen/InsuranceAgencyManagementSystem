namespace IAMS.MultiTenancy.Interfaces
{
    public interface IModuleService
    {
        Task<bool> IsModuleEnabledAsync(string moduleName);
        IEnumerable<string> GetEnabledModules();
        Task<List<string>> GetEnabledModulesAsync();
        Task EnableModuleAsync(string moduleName);
        Task DisableModuleAsync(string moduleName);
        Task<Dictionary<string, bool>> GetAllModulesStatusAsync();
    }
}