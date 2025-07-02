using Microsoft.AspNetCore.Authorization;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.Extensions.Logging;

namespace IAMS.Web.Authorization
{
    public class ModuleAuthorizationHandler : AuthorizationHandler<ModuleRequirement>
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ILogger<ModuleAuthorizationHandler> _logger;

        public ModuleAuthorizationHandler(
            ITenantContextAccessor tenantContextAccessor,
            ILogger<ModuleAuthorizationHandler> logger)
        {
            _tenantContextAccessor = tenantContextAccessor;
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ModuleRequirement requirement)
        {
            // Check if user is authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogDebug("User is not authenticated for module {ModuleName}", requirement.ModuleName);
                context.Fail();
                return Task.CompletedTask;
            }

            // Check if the module is enabled for the current tenant
            if (!_tenantContextAccessor.IsModuleEnabled(requirement.ModuleName))
            {
                _logger.LogWarning("Module {ModuleName} is not enabled for tenant {TenantId}",
                    requirement.ModuleName, _tenantContextAccessor.CurrentTenantId);
                context.Fail();
                return Task.CompletedTask;
            }

            _logger.LogDebug("Module {ModuleName} access granted for tenant {TenantId}",
                requirement.ModuleName, _tenantContextAccessor.CurrentTenantId);
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}