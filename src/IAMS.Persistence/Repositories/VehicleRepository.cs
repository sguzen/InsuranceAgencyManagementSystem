using IAMS.Shared.Interfaces.Repositories;
using IAMS.Domain.Entities;
using IAMS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Persistence.Repositories
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Vehicle?> GetByPlateNumberAsync(string plateNumber)
        {
            return await _dbSet
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Customer)
                .Include(v => v.Currency)
                .Where(v => !v.IsDeleted && v.PlateNumber == plateNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<Vehicle?> GetByChassisNumberAsync(string chassisNumber)
        {
            return await _dbSet
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Customer)
                .Include(v => v.Currency)
                .Where(v => !v.IsDeleted && v.ChassisNumber == chassisNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetByCustomerIdAsync(int customerId)
        {
            return await _dbSet
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Currency)
                .Where(v => !v.IsDeleted && v.CustomerId == customerId)
                .OrderByDescending(v => v.CreatedOn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetVehiclesDueForInspectionAsync(int daysAhead)
        {
            var targetDate = DateTime.Today.AddDays(daysAhead);
            return await _dbSet
                .Include(v => v.Brand)
                .Include(v => v.Model)
                .Include(v => v.Customer)
                .Where(v => !v.IsDeleted &&
                           v.NextInspectionDate.HasValue &&
                           v.NextInspectionDate.Value <= targetDate)
                .OrderBy(v => v.NextInspectionDate)
                .ToListAsync();
        }

        public async Task<bool> PlateNumberExistsAsync(string plateNumber, int? excludeVehicleId = null)
        {
            var query = _dbSet.Where(v => !v.IsDeleted && v.PlateNumber == plateNumber);

            if (excludeVehicleId.HasValue)
            {
                query = query.Where(v => v.Id != excludeVehicleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ChassisNumberExistsAsync(string chassisNumber, int? excludeVehicleId = null)
        {
            var query = _dbSet.Where(v => !v.IsDeleted && v.ChassisNumber == chassisNumber);

            if (excludeVehicleId.HasValue)
            {
                query = query.Where(v => v.Id != excludeVehicleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Dictionary<string, int>> GetVehicleCountByBrandAsync()
        {
            return await _dbSet
                .Where(v => !v.IsDeleted)
                .Include(v => v.Brand)
                .GroupBy(v => v.Brand.Name)
                .Select(g => new { Brand = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Brand, x => x.Count);
        }

        public async Task<Dictionary<int, int>> GetVehicleCountByYearAsync()
        {
            return await _dbSet
                .Where(v => !v.IsDeleted && v.ModelYear.HasValue)
                .GroupBy(v => v.ModelYear!.Value)
                .Select(g => new { Year = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Year, x => x.Count);
        }
    }
}
