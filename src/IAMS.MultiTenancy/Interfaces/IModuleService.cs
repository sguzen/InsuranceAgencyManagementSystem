namespace IAMS.MultiTenancy.Interfaces
{
    public interface IModuleService
    {
        bool IsModuleEnabled(string moduleName);
        IEnumerable<string> GetEnabledModules();
    }
}