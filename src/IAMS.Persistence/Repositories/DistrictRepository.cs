using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class DistrictRepository : Repository<District>, IDistrictRepository
    {
        public DistrictRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<District>> GetActiveDistrictsAsync()
        {
            return await _dbSet
                .Where(d => d.IsActive && !d.IsDeleted)
                .Include(d => d.City)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<District>> GetByCityIdAsync(int cityId)
        {
            return await _dbSet
                .Where(d => d.CityId == cityId && d.IsActive && !d.IsDeleted)
                .Include(d => d.City)
                .OrderBy(d => d.DisplayOrder)
                .ToListAsync();
        }

        public async Task<District?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(d => d.City)
                .FirstOrDefaultAsync(d => d.Code == code && !d.IsDeleted);
        }
    }
}