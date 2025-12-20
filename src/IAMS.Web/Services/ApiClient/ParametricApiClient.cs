using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface IParametricApiClient
    {
        Task<Result<CountryDataSyncResult>> SyncCountryDataAsync(bool updateExisting = true, bool deactivateMissing = false);
        Task<Result> TestCountryConnectionAsync();
        Task<Result<List<ExternalCountryDataDto>>> FetchExternalCountryDataAsync();
    }

    public class ParametricApiClient : BaseApiClient, IParametricApiClient
    {
        public ParametricApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<CountryDataSyncResult>> SyncCountryDataAsync(bool updateExisting = true, bool deactivateMissing = false)
        {
            return await PostAsync<CountryDataSyncResult>(
                $"api/parametric/countries/sync?updateExisting={updateExisting}&deactivateMissing={deactivateMissing}",
                null);
        }

        public async Task<Result> TestCountryConnectionAsync()
        {
            return await GetAsync<Result>("api/parametric/countries/test-connection")
                .ContinueWith(t => t.Result.IsSuccess ? Result.Success(t.Result.Data?.Message ?? "Connected") : Result.Failure(t.Result.Message));
        }

        public async Task<Result<List<ExternalCountryDataDto>>> FetchExternalCountryDataAsync()
        {
            return await GetAsync<List<ExternalCountryDataDto>>("api/parametric/countries/fetch-external-data");
        }
    }
}
