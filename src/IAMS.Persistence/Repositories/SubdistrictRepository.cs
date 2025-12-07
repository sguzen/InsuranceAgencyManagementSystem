using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class SubdistrictRepository : Repository<Subdistrict>, ISubdistrictRepository
    {
        public SubdistrictRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Subdistrict>> GetActiveSubdistrictsAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive && !s.IsDeleted)
                .Include(s => s.District)
                    .ThenInclude(d => d.City)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Subdistrict>> GetByDistrictIdAsync(int districtId)
        {
            return await _dbSet
                .Where(s => s.DistrictId == districtId && s.IsActive && !s.IsDeleted)
                .Include(s => s.District)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Subdistrict?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(s => s.District)
                .FirstOrDefaultAsync(s => s.Code == code && !s.IsDeleted);
        }
    }
}