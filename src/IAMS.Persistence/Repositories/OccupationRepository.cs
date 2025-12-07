using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class OccupationRepository : Repository<Occupation>, IOccupationRepository
    {
        public OccupationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Occupation>> GetActiveOccupationsAsync()
        {
            return await _dbSet
                .Where(o => o.IsActive && !o.IsDeleted)
                .OrderBy(o => o.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Occupation?> GetByCodeAsync(string code)
        {
            return await FirstOrDefaultAsync(o => o.Code == code);
        }
    }
}