namespace IAMS.Application.Interfaces
{
    public interface IModuleService
    {
        Task<bool> IsModuleEnabledAsync(string moduleName);
        Task<List<string>> GetEnabledModulesAsync();
        Task EnableModuleAsync(string moduleName);
        Task DisableModuleAsync(string moduleName);
        Task<Dictionary<string, bool>> GetAllModuleStatusesAsync();
    }
}