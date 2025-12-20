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
        Task<Result<List<CountryDto>>> GetCountriesAsync();
        Task<Result<List<CityDto>>> GetCitiesAsync();
        Task<Result<List<DistrictDto>>> GetDistrictsAsync();
        Task<Result<List<SubdistrictDto>>> GetSubdistrictsAsync();
        Task<Result<List<VillageDto>>> GetVillagesAsync();
        Task<Result<List<OccupationDto>>> GetOccupationsAsync();
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
            var result = await GetAsync<string>("api/parametric/countries/test-connection");
            return result.IsSuccess ? Result.Success(result.Message) : Result.Failure(result.Message, (List<string>?)null);
        }

        public async Task<Result<List<ExternalCountryDataDto>>> FetchExternalCountryDataAsync()
        {
            return await GetAsync<List<ExternalCountryDataDto>>("api/parametric/countries/fetch-external-data");
        }

        public async Task<Result<List<CountryDto>>> GetCountriesAsync()
        {
            return await GetAsync<List<CountryDto>>("api/parametric/countries");
        }

        public async Task<Result<List<CityDto>>> GetCitiesAsync()
        {
            return await GetAsync<List<CityDto>>("api/parametric/cities");
        }

        public async Task<Result<List<DistrictDto>>> GetDistrictsAsync()
        {
            return await GetAsync<List<DistrictDto>>("api/parametric/districts");
        }

        public async Task<Result<List<SubdistrictDto>>> GetSubdistrictsAsync()
        {
            return await GetAsync<List<SubdistrictDto>>("api/parametric/subdistricts");
        }

        public async Task<Result<List<VillageDto>>> GetVillagesAsync()
        {
            return await GetAsync<List<VillageDto>>("api/parametric/villages");
        }

        public async Task<Result<List<OccupationDto>>> GetOccupationsAsync()
        {
            return await GetAsync<List<OccupationDto>>("api/parametric/occupations");
        }
    }
}
