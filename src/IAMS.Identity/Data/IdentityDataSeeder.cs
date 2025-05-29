using IAMS.Identity.Models;
using IAMS.MultiTenancy.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Identity.Data
{
    // IAMS.Identity/Data/IdentityDataSeeder.cs
    public class IdentityDataSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationUser> _roleManager;
        private readonly AppIdentityDbContext _dbContext;
        private readonly ITenantService _tenantService;
        private readonly ILogger<IdentityDataSeeder> _logger;

        public IdentityDataSeeder(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationUser> roleManager,
            AppIdentityDbContext dbContext,
            ITenantService tenantService,
            ILogger<IdentityDataSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task SeedAsync(int tenantId)
        {
            try
            {
                // Set tenant context
                var tenant = await _tenantService.GetTenantAsync(tenantId);

                if (tenant == null)
                {
                    _logger.LogError("Cannot seed identity data: Tenant {TenantId} not found", tenantId);
                    return;
                }

                // Seed default permissions
                await SeedPermissionsAsync();

                // Seed default roles
                await SeedRolesAsync(tenantId);

                // Seed admin user
                await SeedAdminUserAsync(tenantId);
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
            if (await _dbContext.Set<Permission>().AnyAsync())
            {
                return;
            }

            // Core permissions
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
            
            // Policy permissions
            new Permission { Name = "policies.view", DisplayName = "View Policies", Description = "Can view policy list and details", IsSystem = true },
            new Permission { Name = "policies.create", DisplayName = "Create Policies", Description = "Can create new policies", IsSystem = true },
            new Permission { Name = "policies.edit", DisplayName = "Edit Policies", Description = "Can edit policy details", IsSystem = true },
            new Permission { Name = "policies.delete", DisplayName = "Delete Policies", Description = "Can delete policies", IsSystem = true },
            
            // Settings permissions
            new Permission { Name = "settings.view", DisplayName = "View Settings", Description = "Can view system settings", IsSystem = true },
            new Permission { Name = "settings.edit", DisplayName = "Edit Settings", Description = "Can edit system settings", IsSystem = true },
            
            // Module-specific permissions can be added here
            // Reporting module permissions
            new Permission { Name = "reports.view", DisplayName = "View Reports", Description = "Can view reports", Module = "Reporting", IsSystem = true },
            new Permission { Name = "reports.create", DisplayName = "Create Reports", Description = "Can create new reports", Module = "Reporting", IsSystem = true },
            new Permission { Name = "reports.export", DisplayName = "Export Reports", Description = "Can export reports", Module = "Reporting", IsSystem = true },
            
            // Accounting module permissions
            new Permission { Name = "accounting.view", DisplayName = "View Accounting", Description = "Can view accounting data", Module = "Accounting", IsSystem = true },
            new Permission { Name = "accounting.create", DisplayName = "Create Transactions", Description = "Can create accounting transactions", Module = "Accounting", IsSystem = true },
            new Permission { Name = "accounting.edit", DisplayName = "Edit Transactions", Description = "Can edit accounting transactions", Module = "Accounting", IsSystem = true },
            
            // Integration module permissions
            new Permission { Name = "integration.view", DisplayName = "View Integrations", Description = "Can view integration status", Module = "Integration", IsSystem = true },
            new Permission { Name = "integration.configure", DisplayName = "Configure Integrations", Description = "Can configure integration settings", Module = "Integration", IsSystem = true },
            new Permission { Name = "integration.sync", DisplayName = "Run Synchronization", Description = "Can run integration synchronization", Module = "Integration", IsSystem = true }
        };

            await _dbContext.Set<Permission>().AddRangeAsync(permissions);
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
            var allPermissions = await _dbContext.Set<Permission>().ToListAsync();
            var corePermissions = allPermissions.Where(p => p.Module == null).ToList();

            // Create Administrator role
            var adminRole = new ApplicationUser
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
                await _roleManager.AddClaimAsync(adminRole, new Claim("Permission", permission.Name));

                // Also add to role permissions table
                _dbContext.Set<RolePermission>().Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }

            // Create Manager role
            var managerRole = new ApplicationUser
            {
                Name = "Manager",
                NormalizedName = "MANAGER",
                Description = "Management access with limited administrative functions",
                IsDefault = false,
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(managerRole);

            // Assign view and edit permissions to manager role
            var managerPermissions = allPermissions.Where(p =>
                p.Name.EndsWith(".view") ||
                p.Name.EndsWith(".edit") ||
                p.Name.EndsWith(".create") ||
                p.Name == "reports.export").ToList();

            foreach (var permission in managerPermissions)
            {
                await _roleManager.AddClaimAsync(managerRole, new Claim("Permission", permission.Name));

                _dbContext.Set<RolePermission>().Add(new RolePermission
                {
                    RoleId = managerRole.Id,
                    PermissionId = permission.Id
                });
            }

            // Create Agent role
            var agentRole = new ApplicationUser
            {
                Name = "Agent",
                NormalizedName = "AGENT",
                Description = "Standard user with customer and policy management",
                IsDefault = true, // Default role for new users
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(agentRole);

            // Assign basic permissions to agent role
            var agentPermissions = new[]
            {
            "customers.view", "customers.create", "customers.edit",
            "policies.view", "policies.create", "policies.edit",
            "reports.view", "reports.export"
        };

            foreach (var permissionName in agentPermissions)
            {
                var permission = allPermissions.FirstOrDefault(p => p.Name == permissionName);

                if (permission != null)
                {
                    await _roleManager.AddClaimAsync(agentRole, new Claim("Permission", permission.Name));

                    _dbContext.Set<RolePermission>().Add(new RolePermission
                    {
                        RoleId = agentRole.Id,
                        PermissionId = permission.Id
                    });
                }
            }

            // Create ReadOnly role
            var readOnlyRole = new ApplicationUser
            {
                Name = "ReadOnly",
                NormalizedName = "READONLY",
                Description = "Read-only access to system data",
                IsDefault = false,
                IsSystem = true,
                TenantId = tenantId
            };

            await _roleManager.CreateAsync(readOnlyRole);

            // Assign view permissions to read-only role
            // Assign view permissions to read-only role
            var viewPermissions = allPermissions.Where(p => p.Name.EndsWith(".view")).ToList();

            foreach (var permission in viewPermissions)
            {
                await _roleManager.AddClaimAsync(readOnlyRole, new Claim("Permission", permission.Name));

                _dbContext.Set<RolePermission>().Add(new RolePermission
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
            var adminEmail = $"admin@tenant{tenantId}.com";
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
        }
    }
}
