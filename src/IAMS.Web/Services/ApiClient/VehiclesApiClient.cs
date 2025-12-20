using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IVehiclesApiClient
    {
        Task<Result<VehicleDataSyncResult>> SyncVehicleDataAsync(bool updateExisting = true, bool deactivateMissing = false);
        Task<Result> TestConnectionAsync();
        Task<Result<List<ExternalVehicleDataDto>>> FetchExternalDataAsync();
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
    }
}
