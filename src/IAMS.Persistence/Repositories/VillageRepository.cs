using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class VillageRepository : Repository<Village>, IVillageRepository
    {
        public VillageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Village>> GetActiveVillagesAsync()
        {
            return await _dbSet
                .Where(v => v.IsActive && !v.IsDeleted)
                .Include(v => v.Subdistrict)
                    .ThenInclude(s => s.District)
                        .ThenInclude(d => d.City)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Village>> GetBySubdistrictIdAsync(int subdistrictId)
        {
            return await _dbSet
                .Where(v => v.SubdistrictId == subdistrictId && v.IsActive && !v.IsDeleted)
                .Include(v => v.Subdistrict)
                .OrderBy(v => v.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Village?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(v => v.Subdistrict)
                .FirstOrDefaultAsync(v => v.Code == code && !v.IsDeleted);
        }
    }
}