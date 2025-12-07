using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class VehicleBrandRepository : Repository<VehicleBrand>, IVehicleBrandRepository
    {
        public VehicleBrandRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VehicleBrand>> GetActiveBrandsAsync()
        {
            return await _dbSet
                .Where(b => !b.IsDeleted && b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<bool> BrandNameExistsAsync(string name, int? excludeBrandId = null)
        {
            var query = _dbSet.Where(b => !b.IsDeleted && b.Name == name);

            if (excludeBrandId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBrandId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<VehicleBrand?> GetByNameAsync(string name)
        {
            return await _dbSet
                .Where(b => !b.IsDeleted && b.Name == name)
                .FirstOrDefaultAsync();
        }
    }
}
