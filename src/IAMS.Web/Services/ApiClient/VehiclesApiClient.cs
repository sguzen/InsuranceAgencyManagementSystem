using IAMS.Shared.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IVehiclesApiClient
    {
        Task<Result<VehicleDataSyncResult>> SyncVehicleDataAsync(bool updateExisting = true, bool deactivateMissing = false);
        Task<Result> TestConnectionAsync();
        Task<Result<List<ExternalVehicleDataDto>>> FetchExternalDataAsync();
        Task<Result<List<VehicleBrandDto>>> GetActiveBrandsAsync();
        Task<Result<List<VehicleModelDto>>> GetActiveModelsByBrandIdAsync(int brandId);
        Task<Result<List<VehicleDto>>> GetByCustomerIdAsync(int customerId);
        Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto vehicleDto);
    }

    public class VehiclesApiClient : BaseApiClient, IVehiclesApiClient
    {
        public VehiclesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<VehicleDataSyncResult>> SyncVehicleDataAsync(bool updateExisting = true, bool deactivateMissing = false)
        {
            return await PostAsync<VehicleDataSyncResult>(
                $"api/vehicles/sync?updateExisting={updateExisting}&deactivateMissing={deactivateMissing}",
                null);
        }

        public async Task<Result> TestConnectionAsync()
        {
            var result = await GetAsync<string>("api/vehicles/test-connection");
            return result.IsSuccess ? Result.Success(result.Message) : Result.Failure(result.Message, (List<string>?)null);
        }

        public async Task<Result<List<ExternalVehicleDataDto>>> FetchExternalDataAsync()
        {
            return await GetAsync<List<ExternalVehicleDataDto>>("api/vehicles/fetch-external-data");
        }

        public async Task<Result<List<VehicleBrandDto>>> GetActiveBrandsAsync()
        {
            return await GetAsync<List<VehicleBrandDto>>("api/vehicles/brands/active");
        }

        public async Task<Result<List<VehicleModelDto>>> GetActiveModelsByBrandIdAsync(int brandId)
        {
            return await GetAsync<List<VehicleModelDto>>($"api/vehicles/brands/{brandId}/models/active");
        }

        public async Task<Result<List<VehicleDto>>> GetByCustomerIdAsync(int customerId)
        {
            return await GetAsync<List<VehicleDto>>($"api/vehicles/customer/{customerId}");
        }

        public async Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto vehicleDto)
        {
            return await PostAsync<VehicleDto>("api/vehicles", vehicleDto);
        }
    }
}
