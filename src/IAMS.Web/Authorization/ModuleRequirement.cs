using Microsoft.AspNetCore.Authorization;

namespace IAMS.Web.Authorization
{
    public class ModuleRequirement : IAuthorizationRequirement
    {
        public string ModuleName { get; }

        public ModuleRequirement(string moduleName)
        {
            ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
        }
    }
}
