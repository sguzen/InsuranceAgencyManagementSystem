using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class VehicleModelRepository : Repository<VehicleModel>, IVehicleModelRepository
    {
        public VehicleModelRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<VehicleModel>> GetByBrandIdAsync(int brandId)
        {
            return await _dbSet
                .Include(m => m.Brand)
                .Where(m => !m.IsDeleted && m.BrandId == brandId)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehicleModel>> GetActiveByBrandIdAsync(int brandId)
        {
            return await _dbSet
                .Include(m => m.Brand)
                .Where(m => !m.IsDeleted && m.BrandId == brandId && m.IsActive)
                .OrderBy(m => m.DisplayOrder)
                .ThenBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<bool> ModelNameExistsAsync(int brandId, string name, int? excludeModelId = null)
        {
            var query = _dbSet.Where(m => !m.IsDeleted && m.BrandId == brandId && m.Name == name);

            if (excludeModelId.HasValue)
            {
                query = query.Where(m => m.Id != excludeModelId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<VehicleModel?> GetByNameAndBrandIdAsync(string name, int brandId)
        {
            return await _dbSet
                .Where(m => !m.IsDeleted && m.Name == name && m.BrandId == brandId)
                .FirstOrDefaultAsync();
        }
    }
}
