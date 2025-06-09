namespace IAMS.Application.Exceptions
{
    public class ModuleNotEnabledException : ApplicationException
    {
        public string ModuleName { get; }

        public ModuleNotEnabledException(string moduleName)
            : base($"Module '{moduleName}' is not enabled for this tenant.")
        {
            ModuleName = moduleName;
        }

        public ModuleNotEnabledException(string moduleName, string message) : base(message)
        {
            ModuleName = moduleName;
        }
    }
}
