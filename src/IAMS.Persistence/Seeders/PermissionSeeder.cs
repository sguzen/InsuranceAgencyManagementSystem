using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Persistence.Seeders
{
    public static class PermissionSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
                {
                    new() { Name = "admin", DisplayName = "Admin", Module = "All", IsSystem = true },
                    new() { Name = "customers.view", DisplayName = "View Customers", Module = "Core", IsSystem = true },
                };

                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
