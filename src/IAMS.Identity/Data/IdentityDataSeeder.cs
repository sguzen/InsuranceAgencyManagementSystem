using IAMS.Application.Constants;
using IAMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IAMS.Identity.Data
{
    public static class IdentityDataSeeder
    {
        public static async Task SeedRolesAndPermissionsAsync(
            RoleManager<ApplicationRole> roleManager,
            ILogger logger)
        {
            // Define roles with their associated permissions
            var rolesWithPermissions = new Dictionary<string, List<string>>
            {
                {
                    RoleNames.SuperAdmin,
                    new List<string>
                    {
                        // SuperAdmin has ALL permissions
                        PermissionNames.ViewCustomers, PermissionNames.CreateCustomers, PermissionNames.EditCustomers, PermissionNames.DeleteCustomers,
                        PermissionNames.ViewPolicies, PermissionNames.CreatePolicies, PermissionNames.EditPolicies, PermissionNames.DeletePolicies, PermissionNames.RenewPolicies,
                        PermissionNames.ViewInsuranceCompanies, PermissionNames.CreateInsuranceCompanies, PermissionNames.EditInsuranceCompanies, PermissionNames.DeleteInsuranceCompanies,
                        PermissionNames.ViewPayments, PermissionNames.CreatePayments, PermissionNames.EditPayments, PermissionNames.DeletePayments,
                        PermissionNames.ViewClaims, PermissionNames.CreateClaims, PermissionNames.EditClaims, PermissionNames.DeleteClaims,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports,
                        PermissionNames.ViewAccounting, PermissionNames.ManageCommissions, PermissionNames.ViewFinancials,
                        PermissionNames.ViewIntegrations, PermissionNames.ManageIntegrations, PermissionNames.SyncData,
                        PermissionNames.ManageUsers, PermissionNames.ManageRoles, PermissionNames.ManagePermissions, PermissionNames.ManageSettings, PermissionNames.ManageTenant, PermissionNames.ManageModules
                    }
                },
                {
                    RoleNames.TenantAdmin,
                    new List<string>
                    {
                        // Tenant Admin - All except SuperAdmin functions
                        PermissionNames.ViewCustomers, PermissionNames.CreateCustomers, PermissionNames.EditCustomers, PermissionNames.DeleteCustomers,
                        PermissionNames.ViewPolicies, PermissionNames.CreatePolicies, PermissionNames.EditPolicies, PermissionNames.DeletePolicies, PermissionNames.RenewPolicies,
                        PermissionNames.ViewInsuranceCompanies, PermissionNames.CreateInsuranceCompanies, PermissionNames.EditInsuranceCompanies, PermissionNames.DeleteInsuranceCompanies,
                        PermissionNames.ViewPayments, PermissionNames.CreatePayments, PermissionNames.EditPayments, PermissionNames.DeletePayments,
                        PermissionNames.ViewClaims, PermissionNames.CreateClaims, PermissionNames.EditClaims, PermissionNames.DeleteClaims,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports,
                        PermissionNames.ViewAccounting, PermissionNames.ManageCommissions, PermissionNames.ViewFinancials,
                        PermissionNames.ManageUsers, PermissionNames.ManageRoles, PermissionNames.ManageSettings, PermissionNames.ManageModules
                    }
                },
                {
                    RoleNames.Manager,
                    new List<string>
                    {
                        // Manager - Can manage operations but not users/system settings
                        PermissionNames.ViewCustomers, PermissionNames.CreateCustomers, PermissionNames.EditCustomers,
                        PermissionNames.ViewPolicies, PermissionNames.CreatePolicies, PermissionNames.EditPolicies, PermissionNames.RenewPolicies,
                        PermissionNames.ViewInsuranceCompanies, PermissionNames.CreateInsuranceCompanies, PermissionNames.EditInsuranceCompanies,
                        PermissionNames.ViewPayments, PermissionNames.CreatePayments, PermissionNames.EditPayments,
                        PermissionNames.ViewClaims, PermissionNames.CreateClaims, PermissionNames.EditClaims,
                        PermissionNames.ViewReports, PermissionNames.CreateReports, PermissionNames.ExportReports,
                        PermissionNames.ViewAccounting, PermissionNames.ManageCommissions
                    }
                },
                {
                    RoleNames.Agent,
                    new List<string>
                    {
                        // Agent - Can create and manage policies/customers
                        PermissionNames.ViewCustomers, PermissionNames.CreateCustomers, PermissionNames.EditCustomers,
                        PermissionNames.ViewPolicies, PermissionNames.CreatePolicies, PermissionNames.EditPolicies, PermissionNames.RenewPolicies,
                        PermissionNames.ViewInsuranceCompanies,
                        PermissionNames.ViewPayments, PermissionNames.CreatePayments,
                        PermissionNames.ViewClaims, PermissionNames.CreateClaims,
                        PermissionNames.ViewReports
                    }
                },
                {
                    RoleNames.AccountingClerk,
                    new List<string>
                    {
                        // Accounting Clerk - Focus on financials
                        PermissionNames.ViewCustomers,
                        PermissionNames.ViewPolicies,
                        PermissionNames.ViewPayments, PermissionNames.CreatePayments, PermissionNames.EditPayments,
                        PermissionNames.ViewAccounting, PermissionNames.ManageCommissions, PermissionNames.ViewFinancials,
                        PermissionNames.ViewReports, PermissionNames.ExportReports
                    }
                },
                {
                    RoleNames.ReportsViewer,
                    new List<string>
                    {
                        // Reports Viewer - Read-only access for reporting
                        PermissionNames.ViewCustomers,
                        PermissionNames.ViewPolicies,
                        PermissionNames.ViewInsuranceCompanies,
                        PermissionNames.ViewPayments,
                        PermissionNames.ViewClaims,
                        PermissionNames.ViewReports, PermissionNames.ExportReports,
                        PermissionNames.ViewAccounting, PermissionNames.ViewFinancials
                    }
                },
                {
                    RoleNames.Viewer,
                    new List<string>
                    {
                        // Viewer - Read-only access
                        PermissionNames.ViewCustomers,
                        PermissionNames.ViewPolicies,
                        PermissionNames.ViewInsuranceCompanies,
                        PermissionNames.ViewPayments,
                        PermissionNames.ViewClaims
                    }
                }
            };

            // Seed roles
            foreach (var (roleName, permissions) in rolesWithPermissions)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    logger.LogInformation("Creating role: {RoleName}", roleName);
                    var role = new ApplicationRole { Name = roleName };
                    var result = await roleManager.CreateAsync(role);

                    if (!result.Succeeded)
                    {
                        logger.LogError("Failed to create role {RoleName}: {Errors}",
                            roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                        continue;
                    }

                    // Add permissions as claims to the role
                    foreach (var permission in permissions)
                    {
                        var claimResult = await roleManager.AddClaimAsync(role,
                            new System.Security.Claims.Claim("permission", permission));

                        if (!claimResult.Succeeded)
                        {
                            logger.LogWarning("Failed to add permission {Permission} to role {RoleName}",
                                permission, roleName);
                        }
                    }

                    logger.LogInformation("Role {RoleName} created with {Count} permissions",
                        roleName, permissions.Count);
                }
                else
                {
                    logger.LogInformation("Role {RoleName} already exists", roleName);
                }
            }
        }
    }
}
