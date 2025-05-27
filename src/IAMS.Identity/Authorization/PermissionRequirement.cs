using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    // IAMS.Identity/Authorization/PermissionHandler.cs
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // Check if user has the required permission claim
            if (context.User.HasClaim(c => c.Type == "Permission" && c.Value == requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    // IAMS.Identity/Authorization/PermissionAuthorizationPolicyProvider.cs
    public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly IConfiguration _configuration;

        public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // If the policy name starts with "Permission", it's a permission policy
            if (policyName.StartsWith("Permission:", StringComparison.OrdinalIgnoreCase))
            {
                var permission = policyName.Substring("Permission:".Length);

                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();

                return policy;
            }

            // Use the base implementation for other policies
            return await base.GetPolicyAsync(policyName);
        }
    }

    // IAMS.Identity/Authorization/HasPermissionAttribute.cs
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false)]
    public class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
            : base("Permission:" + permission)
        {
        }
    }
}
