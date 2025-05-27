using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using IAMS.MultiTenancy.Interfaces;

namespace IAMS.Web.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequiresModuleAttribute : TypeFilterAttribute
    {
        public RequiresModuleAttribute(string moduleName)
            : base(typeof(RequiresModuleFilter))
        {
            Arguments = [moduleName];
        }

        private class RequiresModuleFilter : IAsyncAuthorizationFilter
        {
            private readonly string _moduleName;
            private readonly IModuleService _moduleService;

            public RequiresModuleFilter(string moduleName, IModuleService moduleService)
            {
                _moduleName = moduleName;
                _moduleService = moduleService;
            }

            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                if (!_moduleService.IsModuleEnabled(_moduleName))
                {
                    context.Result = new ForbidResult("Module not enabled");
                }
            }
        }
    }
}
