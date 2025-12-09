using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IAMS.Persistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Permission> _dbSet;

        public PermissionRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<Permission>();
        }

        public async Task<Permission?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Permission>> FindAsync(Expression<Func<Permission, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Permission?> FirstOrDefaultAsync(Expression<Func<Permission, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<Permission> AddAsync(Permission entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task<IEnumerable<Permission>> AddRangeAsync(IEnumerable<Permission> entities)
        {
            var entityList = entities.ToList();
            await _dbSet.AddRangeAsync(entityList);
            return entityList;
        }

        public void Update(Permission entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateRange(IEnumerable<Permission> entities)
        {
            foreach (var entity in entities)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public void Remove(Permission entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<Permission> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Permission, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<Permission, bool>>? predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();

            return await _dbSet.CountAsync(predicate);
        }

        public async Task<PagedResult<Permission>> GetPermissionsPagedAsync(
            int page,
            int pageSize,
            string? module = null)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(module))
            {
                query = query.Where(p => p.Module == module);
            }

            var totalCount = await query.CountAsync();

            var permissions = await query
                .OrderBy(p => p.Module)
                .ThenBy(p => p.DisplayName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Permission>
            {
                Items = permissions,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }

        public async Task<Permission?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludePermissionId = null)
        {
            var query = _dbSet.Where(p => p.Name == name);

            if (excludePermissionId.HasValue)
            {
                query = query.Where(p => p.Id != excludePermissionId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsAssignedToRolesAsync(int permissionId)
        {
            return await _context.RolePermissions
                .AnyAsync(rp => rp.PermissionId == permissionId);
        }

        public async Task<List<string>> GetModulesAsync()
        {
            return await _dbSet
                .Where(p => !string.IsNullOrEmpty(p.Module))
                .Select(p => p.Module!)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<int> SeedDefaultPermissionsAsync()
        {
            var defaultPermissions = new[]
            {
                // Core module permissions
                new Permission { Name = "customers.view", DisplayName = "View Customers", Description = "Can view customer list and details", Module = "Core", IsSystem = true },
                new Permission { Name = "customers.create", DisplayName = "Create Customers", Description = "Can create new customers", Module = "Core", IsSystem = true },
                new Permission { Name = "customers.edit", DisplayName = "Edit Customers", Description = "Can edit customer information", Module = "Core", IsSystem = true },
                new Permission { Name = "customers.delete", DisplayName = "Delete Customers", Description = "Can delete customers", Module = "Core", IsSystem = true },

                new Permission { Name = "policies.view", DisplayName = "View Policies", Description = "Can view policy list and details", Module = "Core", IsSystem = true },
                new Permission { Name = "policies.create", DisplayName = "Create Policies", Description = "Can create new policies", Module = "Core", IsSystem = true },
                new Permission { Name = "policies.edit", DisplayName = "Edit Policies", Description = "Can edit policy information", Module = "Core", IsSystem = true },
                new Permission { Name = "policies.delete", DisplayName = "Delete Policies", Description = "Can delete policies", Module = "Core", IsSystem = true },

                // User management permissions
                new Permission { Name = "users.view", DisplayName = "View Users", Description = "Can view user list and details", Module = "Core", IsSystem = true },
                new Permission { Name = "users.create", DisplayName = "Create Users", Description = "Can create new users", Module = "Core", IsSystem = true },
                new Permission { Name = "users.edit", DisplayName = "Edit Users", Description = "Can edit user information", Module = "Core", IsSystem = true },
                new Permission { Name = "users.delete", DisplayName = "Delete Users", Description = "Can delete users", Module = "Core", IsSystem = true },

                new Permission { Name = "roles.view", DisplayName = "View Roles", Description = "Can view role list and details", Module = "Core", IsSystem = true },
                new Permission { Name = "roles.create", DisplayName = "Create Roles", Description = "Can create new roles", Module = "Core", IsSystem = true },
                new Permission { Name = "roles.edit", DisplayName = "Edit Roles", Description = "Can edit role information", Module = "Core", IsSystem = true },
                new Permission { Name = "roles.delete", DisplayName = "Delete Roles", Description = "Can delete roles", Module = "Core", IsSystem = true },

                // Reporting module permissions
                new Permission { Name = "reports.view", DisplayName = "View Reports", Description = "Can view reports", Module = "Reporting", IsSystem = true },
                new Permission { Name = "reports.create", DisplayName = "Create Reports", Description = "Can create custom reports", Module = "Reporting", IsSystem = true },
                new Permission { Name = "reports.export", DisplayName = "Export Reports", Description = "Can export reports to various formats", Module = "Reporting", IsSystem = true },

                // Accounting module permissions
                new Permission { Name = "accounting.view", DisplayName = "View Accounting", Description = "Can view accounting information", Module = "Accounting", IsSystem = true },
                new Permission { Name = "accounting.manage", DisplayName = "Manage Accounting", Description = "Can manage accounting entries", Module = "Accounting", IsSystem = true },
                new Permission { Name = "commissions.view", DisplayName = "View Commissions", Description = "Can view commission reports", Module = "Accounting", IsSystem = true },
                new Permission { Name = "commissions.manage", DisplayName = "Manage Commissions", Description = "Can manage commission calculations", Module = "Accounting", IsSystem = true },

                // Integration module permissions
                new Permission { Name = "integrations.view", DisplayName = "View Integrations", Description = "Can view integration status", Module = "Integration", IsSystem = true },
                new Permission { Name = "integrations.manage", DisplayName = "Manage Integrations", Description = "Can configure and manage integrations", Module = "Integration", IsSystem = true },
                new Permission { Name = "mappings.manage", DisplayName = "Manage ID Mappings", Description = "Can manage customer ID mappings", Module = "Integration", IsSystem = true },

                // Admin permissions
                new Permission { Name = "admin.system", DisplayName = "System Administration", Description = "Full system administration access", Module = "Admin", IsSystem = true },
                new Permission { Name = "admin.tenants", DisplayName = "Tenant Management", Description = "Can manage tenant settings", Module = "Admin", IsSystem = true },
                new Permission { Name = "admin.modules", DisplayName = "Module Management", Description = "Can enable/disable modules", Module = "Admin", IsSystem = true }
            };

            var addedCount = 0;
            foreach (var permission in defaultPermissions)
            {
                var exists = await _dbSet.AnyAsync(p => p.Name == permission.Name);

                if (!exists)
                {
                    await _dbSet.AddAsync(permission);
                    addedCount++;
                }
            }

            return addedCount;
        }

        public IQueryable<Permission> AsQueryable()
        {
            throw new NotImplementedException();
        }
    }
}
