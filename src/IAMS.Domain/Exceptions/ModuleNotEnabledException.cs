namespace IAMS.Domain.Exceptions
{
    public class ModuleNotEnabledException : DomainException
    {
        public string ModuleName { get; }

        public ModuleNotEnabledException(string moduleName, int tenantId)
            : base("MODULE_NOT_ENABLED", $"Module '{moduleName}' is not enabled for this tenant", tenantId)
        {
            ModuleName = moduleName;
        }
    }
}