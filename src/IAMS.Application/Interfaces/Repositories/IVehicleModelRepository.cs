using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IVehicleModelRepository : IRepository<VehicleModel>
    {
        Task<IEnumerable<VehicleModel>> GetByBrandIdAsync(int brandId);
        Task<IEnumerable<VehicleModel>> GetActiveByBrandIdAsync(int brandId);
        Task<bool> ModelNameExistsAsync(int brandId, string name, int? excludeModelId = null);
    }
}
