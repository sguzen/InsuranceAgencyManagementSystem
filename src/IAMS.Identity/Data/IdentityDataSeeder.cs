// IAMS.Identity/Data/IdentityDataSeeder.cs
using IAMS.Identity.Models;
using IAMS.Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace IAMS.Identity.Data
{
    public class IdentityDataSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IdentityDbContext _dbContext;
        private readonly ILogger<IdentityDataSeeder> _logger;

        public IdentityDataSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IdentityDbContext dbContext,
            ILogger<IdentityDataSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SeedAsync(int tenantId)
        {
            try
            {
                // Seed default permissions first (global, not tenant-specific)
                await SeedPermissionsAsync();

                // Seed default roles for the tenant
                await SeedRolesAsync(tenantId);

                // Seed admin user for the tenant
                await SeedAdminUserAsync(tenantId);

                _logger.LogInformation("Successfully seeded identity data for tenant {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding identity data for tenant {TenantId}", tenantId);
                throw;
            }
        }

        private async Task SeedPermissionsAsync()
        {
            // Check if permissions are already seeded
            if (await _dbContext.Permissions.AnyAsync())
            {
                return;
            }

            // Core permissions for insurance agency system
            var permissions = new List<Permission>
            {
                // User management permissions
                new Permission { Name = "users.view", DisplayName = "View Users", Description = "Can view user list and details", IsSystem = true },
                new Permission { Name = "users.create", DisplayName = "Create Users", Description = "Can create new users", IsSystem = true },
                new Permission { Name = "users.edit", DisplayName = "Edit Users", Description = "Can edit user details", IsSystem = true },
                new Permission { Name = "users.delete", DisplayName = "Delete Users", Description = "Can delete users", IsSystem = true },
                
                // Role management permissions
                new Permission { Name = "roles.view", DisplayName = "View Roles", Description = "Can view role list and details", IsSystem = true },
                new Permission { Name = "roles.create", DisplayName = "Create Roles", Description = "Can create new roles", IsSystem = true },
                new Permission { Name = "roles.edit", DisplayName = "Edit Roles", Description = "Can edit role details", IsSystem = true },
                new Permission { Name = "roles.delete", DisplayName = "Delete Roles", Description = "Can delete roles", IsSystem = true },
                
                // Customer permissions
                new Permission { Name = "customers.view", DisplayName = "View Customers", Description = "Can view customer list and details", IsSystem = true },
                new Permission { Name = "customers.create", DisplayName = "Create Customers", Description = "Can create new customers", IsSystem = true },
                new Permission { Name = "customers.edit", DisplayName = "Edit Customers", Description = "Can edit customer details", IsSystem = true },
                new Permission { Name = "customers.delete", DisplayName = "Delete Customers", Description = "Can delete customers", IsSystem = true },
                new Permission { Name = "customers.export", DisplayName = "Export Customers", Description = "Can export customer data", IsSystem = true },
                
                // Policy permissions
                new Permission { Name = "policies.view", DisplayName = "View Policies", Description = "Can view policy list and details", IsSystem = true },
                new Permission { Name = "policies.create", DisplayName = "Create Policies", Description = "Can create new policies", IsSystem = true },
                new Permission { Name = "policies.edit", DisplayName = "Edit Policies", Description = "Can edit policy details", IsSystem = true },
                new Permission { Name = "policies.delete", DisplayName = "Delete Policies", Description = "Can delete policies", IsSystem = true },
                new Permission { Name = "policies.renew", DisplayName = "Renew Policies", Description = "Can renew policies", IsSystem = true },
                new Permission { Name = "policies.cancel", DisplayName = "Cancel Policies", Description = "Can cancel policies", IsSystem = true },
                new Permission { Name = "policies.export", DisplayName = "Export Policies", Description = "Can export policy data", IsSystem = true },
                
                // Claims permissions
                new Permission { Name = "claims.view", DisplayName = "View Claims", Description = "Can view claims list and details", IsSystem = true },
                new Permission { Name = "claims.create", DisplayName = "Create Claims", Description = "Can create new claims", IsSystem = true },
                new Permission { Name = "claims.edit", DisplayName = "Edit Claims", Description = "Can edit claim details", IsSystem = true },
                new Permission { Name = "claims.process", DisplayName = "Process Claims", Description = "Can process and approve claims", IsSystem = true },
                
                // Settings permissions
                new Permission { Name = "settings.view", DisplayName = "View Settings", Description = "Can view system settings", IsSystem = true },
                new Permission { Name = "settings.edit", DisplayName = "Edit Settings", Description = "Can edit system settings", IsSystem = true },
                new Permission { Name = "settings.backup", DisplayName = "Backup Settings", Description = "Can create system backups", IsSystem = true },
                
                // Dashboard permissions
                new Permission { Name = "dashboard.view", DisplayName = "View Dashboard", Description = "Can view main dashboard", IsSystem = true },
                new Permission { Name = "dashboard.analytics", DisplayName = "View Analytics", Description = "Can view detailed analytics", IsSystem = true },
                
                // Reporting module permissions
                new Permission { Name = "reports.view", DisplayName = "View Reports", Description = "Can view reports", Module = "Reporting", IsSystem = true },
                new Permission { Name = "reports.create", DisplayName = "Create Reports", Description = "Can create custom reports", Module = "Reporting", IsSystem = true },
                new Permission { Name = "reports.export", DisplayName = "Export Reports", Description = "Can export reports", Module = "Reporting", IsSystem = true },
                new Permission { Name = "reports.schedule", DisplayName = "Schedule Reports", Description = "Can schedule automated reports", Module = "Reporting", IsSystem = true },
                
                // Accounting module permissions
                new Permission { Name = "accounting.view", DisplayName = "View Accounting", Description = "Can view accounting data", Module = "Accounting", IsSystem = true },
                new Permission { Name = "accounting.transactions", DisplayName = "Manage Transactions", Description = "Can create and edit accounting transactions", Module = "Accounting", IsSystem = true },
                new Permission { Name = "accounting.reconcile", DisplayName = "Reconcile Accounts", Description = "Can reconcile accounts", Module = "Accounting", IsSystem = true },
                new Permission { Name = "accounting.reports", DisplayName = "Accounting Reports", Description = "Can generate accounting reports", Module = "Accounting", IsSystem = true },
                
                // Integration module permissions
                new Permission { Name = "integration.view", DisplayName = "View Integrations", Description = "Can view integration status", Module = "Integration", IsSystem = true },
                new Permission { Name = "integration.configure", DisplayName = "Configure Integrations", Description = "Can configure integration settings", Module = "Integration", IsSystem = true },
                new Permission { Name = "integration.sync", DisplayName = "Run Synchronization", Description = "Can run integration synchronization", Module = "Integration", IsSystem = true },
                new Permission { Name = "integration.logs", DisplayName = "View Integration Logs", Description = "Can view integration logs and errors", Module = "Integration", IsSystem = true }
            };

            await _dbContext.Permissions.AddRangeAsync(permissions);
            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedRolesAsync(int tenantId)
        {
            // Check if this tenant already has roles
            var hasRoles = await _roleManager.Roles.AnyAsync(r => r.TenantId == tenantId);

            if (hasRoles)
            {
                return;
            }

            // Get all permissions
            var allPermissions = await _dbContext.Permissions.ToListAsync();

            // Create Administrator role
            var adminRole = new ApplicationRole
            {
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                Description = "Full access to all system functions",
                IsDefault = false,
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(adminRole);

            // Assign all permissions to admin role
            foreach (var permission in allPermissions)
            {
                await _roleManager.AddClaimAsync(adminRole, new Claim("permission", permission.Name));

                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // Create Manager role
            var managerRole = new ApplicationRole
            {
                Name = "Manager",
                NormalizedName = "MANAGER",
                Description = "Management access with limited administrative functions",
                IsDefault = false,
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(managerRole);

            // Manager permissions (all core permissions except user/role management delete)
            var managerPermissionNames = new[]
            {
                "dashboard.view", "dashboard.analytics",
                "customers.view", "customers.create", "customers.edit", "customers.export",
                "policies.view", "policies.create", "policies.edit", "policies.renew", "policies.cancel", "policies.export",
                "claims.view", "claims.create", "claims.edit", "claims.process",
                "users.view", "users.create", "users.edit",
                "roles.view",
                "settings.view", "settings.edit",
                "reports.view", "reports.create", "reports.export", "reports.schedule",
                "accounting.view", "accounting.transactions", "accounting.reports",
                "integration.view", "integration.sync", "integration.logs"
            };

            foreach (var permissionName in managerPermissionNames)
            {
                var permission = allPermissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null)
                {
                    await _roleManager.AddClaimAsync(managerRole, new Claim("permission", permission.Name));

                    _dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = managerRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Create Agent role
            var agentRole = new ApplicationRole
            {
                Name = "Agent",
                NormalizedName = "AGENT",
                Description = "Standard agent with customer and policy management",
                IsDefault = true, // Default role for new users
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(agentRole);

            // Agent permissions (core business functions)
            var agentPermissionNames = new[]
            {
                "dashboard.view",
                "customers.view", "customers.create", "customers.edit",
                "policies.view", "policies.create", "policies.edit", "policies.renew",
                "claims.view", "claims.create", "claims.edit",
                "reports.view", "reports.export"
            };

            foreach (var permissionName in agentPermissionNames)
            {
                var permission = allPermissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null)
                {
                    await _roleManager.AddClaimAsync(agentRole, new Claim("permission", permission.Name));

                    _dbContext.RolePermissions.Add(new RolePermission
                    {
                        RoleId = agentRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Create ReadOnly role
            var readOnlyRole = new ApplicationRole
            {
                Name = "ReadOnly",
                NormalizedName = "READONLY",
                Description = "Read-only access to system data",
                IsDefault = false,
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(readOnlyRole);

            // ReadOnly permissions (only view permissions)
            var viewPermissions = allPermissions.Where(p => p.Name.EndsWith(".view")).ToList();

            foreach (var permission in viewPermissions)
            {
                await _roleManager.AddClaimAsync(readOnlyRole, new Claim("permission", permission.Name));

                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = readOnlyRole.Id,
                    PermissionId = permission.Id
                });
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task SeedAdminUserAsync(int tenantId)
        {
            // Check if admin user already exists
            var adminEmail = $"admin@tenant{tenantId}.local";
            var adminExists = await _userManager.Users.AnyAsync(u => u.Email == adminEmail && u.TenantId == tenantId);

            if (adminExists)
            {
                return;
            }

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                TenantId = tenantId,
                CreatedOn = DateTime.UtcNow
            };

            // Use a secure default password (should be changed on first login)
            var createResult = await _userManager.CreateAsync(adminUser, "Admin@123456");

            if (!createResult.Succeeded)
            {
                throw new Exception($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }

            // Assign administrator role
            await _userManager.AddToRoleAsync(adminUser, "Administrator");

            _logger.LogInformation("Created admin user {Email} for tenant {TenantId}", adminEmail, tenantId);
        }
    }
}