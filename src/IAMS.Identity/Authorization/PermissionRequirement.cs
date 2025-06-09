// IAMS.Identity/Authorization/PermissionRequirement.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace IAMS.Identity.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Check if user has the required permission claim (consistent case)
            if (context.User.HasClaim(c => c.Type == "permission" && c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IConfiguration _configuration;

        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // If the policy name starts with "Permission:", it's a permission policy
            if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring("Permission:".Length);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();

                return policy;
            }

            // If the policy name starts with "Module:", it's a module policy
            if (policyName.StartsWith("Module:", StringComparison.OrdinalIgnoreCase))
            {
                var moduleName = policyName.Substring("Module:".Length);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new ModuleRequirement(moduleName))
                    .Build();

                return policy;
            }

            // Use the base implementation for other policies
            return await base.GetPolicyAsync(policyName);
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
            : base("Permission:" + permission)
        {
        }
    }
}