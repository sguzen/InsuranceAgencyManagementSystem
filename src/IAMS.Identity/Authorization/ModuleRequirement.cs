using IAMS.MultiTenancy.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Identity.Authorization
{
    public class ModuleRequirement : IAuthorizationRequirement
    {
        public string ModuleName { get; }

        public ModuleRequirement(string moduleName)
        {
            ModuleName = moduleName;
        }
    }

    // IAMS.Identity/Authorization/ModuleHandler.cs
    public class ModuleHandler : AuthorizationHandler<ModuleRequirement>
    {
        private readonly IModuleService _moduleService;

        public ModuleHandler(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ModuleRequirement requirement)
        {
            // Check if the required module is enabled for the tenant
            if (_moduleService.IsModuleEnabled(requirement.ModuleName))
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail(new AuthorizationFailureReason(this, "Module not enabled"));
            }

            return Task.CompletedTask;
        }
    }

    // IAMS.Identity/Authorization/RequiresModuleAttribute.cs
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class RequiresModuleAttribute : AuthorizeAttribute
    {
        public RequiresModuleAttribute(string moduleName)
            : base("Module:" + moduleName)
        {
        }
    }
}
