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
            return await GetAsync<Result>("api/vehicles/test-connection")
                .ContinueWith(t => t.Result.IsSuccess ? Result.Success(t.Result.Data?.Message ?? "Connected") : Result.Failure(t.Result.Message));
        }

        public async Task<Result<List<ExternalVehicleDataDto>>> FetchExternalDataAsync()
        {
            return await GetAsync<List<ExternalVehicleDataDto>>("api/vehicles/fetch-external-data");
        }
    }
}
