using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface IVehicleBrandRepository : IRepository<VehicleBrand>
    {
        Task<IEnumerable<VehicleBrand>> GetActiveBrandsAsync();
        Task<bool> BrandNameExistsAsync(string name, int? excludeBrandId = null);
    }
}
