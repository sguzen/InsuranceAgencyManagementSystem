using IAMS.Application.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class MarketerRepository : Repository<Marketer>, IMarketerRepository
    {
        public MarketerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Marketer?> GetByCodeAsync(string code)
        {
            return await _context.Set<Marketer>()
                .FirstOrDefaultAsync(m => m.Code == code && !m.IsDeleted);
        }

        public async Task<IEnumerable<Marketer>> GetActiveMarketersAsync()
        {
            return await _context.Set<Marketer>()
                .Where(m => m.IsActive && !m.IsDeleted)
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<(Marketer Marketer, int PolicyCount)>> GetMarketersWithPolicyCountAsync()
        {
            return await _context.Set<Marketer>()
                .Where(m => !m.IsDeleted)
                .Select(m => new
                {
                    Marketer = m,
                    PolicyCount = m.Policies.Count(p => !p.IsDeleted)
                })
                .Select(x => ValueTuple.Create(x.Marketer, x.PolicyCount))
                .ToListAsync();
        }
    }
}
