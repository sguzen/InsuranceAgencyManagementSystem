using IAMS.Shared.DTOs.Vehicle;
using IAMS.Shared.Models;

namespace IAMS.Application.Services.Vehicles
{
    public interface IVehicleService
    {
        // Vehicle CRUD
        Task<Result<VehicleDto>> GetByIdAsync(int id);
        Task<Result<VehicleDto>> GetByPlateNumberAsync(string plateNumber);
        Task<Result<VehicleDto>> GetByChassisNumberAsync(string chassisNumber);
        Task<Result<PagedResult<VehicleDto>>> GetAllAsync(VehicleQueryParams queryParams);
        Task<Result<List<VehicleDto>>> GetByCustomerIdAsync(int customerId);
        Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto dto);
        Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto dto);
        Task<Result> DeleteAsync(int id);

        // Vehicle Brands & Models
        Task<Result<List<VehicleBrandDto>>> GetAllBrandsAsync();
        Task<Result<List<VehicleBrandDto>>> GetActiveBrandsAsync();
        Task<Result<List<VehicleModelDto>>> GetModelsByBrandIdAsync(int brandId);
        Task<Result<List<VehicleModelDto>>> GetActiveModelsByBrandIdAsync(int brandId);

        // Inspection Management
        Task<Result<List<VehicleDto>>> GetVehiclesDueForInspectionAsync(int daysAhead = 30);
        Task<Result> UpdateInspectionDatesAsync(int vehicleId, DateTime? lastInspection, DateTime? nextInspection);

        // Statistics
        Task<Result<int>> GetTotalVehiclesCountAsync();
        Task<Result<Dictionary<string, int>>> GetVehiclesByBrandAsync();
        Task<Result<Dictionary<int, int>>> GetVehiclesByYearAsync();
    }

    public class VehicleQueryParams
    {
        public string? PlateNumber { get; set; }
        public string? ChassisNumber { get; set; }
        public int? CustomerId { get; set; }
        public int? BrandId { get; set; }
        public int? ModelId { get; set; }
        public int? ModelYear { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}