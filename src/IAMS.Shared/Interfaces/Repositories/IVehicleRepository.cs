using IAMS.Domain.Entities;

namespace IAMS.Shared.Interfaces.Repositories
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Task<Vehicle?> GetByPlateNumberAsync(string plateNumber);
        Task<Vehicle?> GetByChassisNumberAsync(string chassisNumber);
        Task<IEnumerable<Vehicle>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Vehicle>> GetVehiclesDueForInspectionAsync(int daysAhead);
        Task<bool> PlateNumberExistsAsync(string plateNumber, int? excludeVehicleId = null);
        Task<bool> ChassisNumberExistsAsync(string chassisNumber, int? excludeVehicleId = null);
        Task<Dictionary<string, int>> GetVehicleCountByBrandAsync();
        Task<Dictionary<int, int>> GetVehicleCountByYearAsync();
    }
}
