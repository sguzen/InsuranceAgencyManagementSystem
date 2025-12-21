using IAMS.Shared.DTOs.Parametric;
using IAMS.Shared.Models;

namespace IAMS.Application.Interfaces
{
    public interface IParametricService
    {
        // Countries
        Task<Result<List<CountryDto>>> GetCountriesAsync();
        Task<Result<CountryDto>> GetCountryByIdAsync(int id);
        Task<Result<CountryDto>> GetCountryByCodeAsync(string code);

        // Occupations
        Task<Result<List<OccupationDto>>> GetOccupationsAsync();
        Task<Result<OccupationDto>> GetOccupationByIdAsync(int id);

        // Cities
        Task<Result<List<CityDto>>> GetCitiesAsync();
        Task<Result<CityDto>> GetCityByIdAsync(int id);

        // Districts
        Task<Result<List<DistrictDto>>> GetDistrictsAsync();
        Task<Result<List<DistrictDto>>> GetDistrictsByCityIdAsync(int cityId);
        Task<Result<DistrictDto>> GetDistrictByIdAsync(int id);

        // Subdistricts
        Task<Result<List<SubdistrictDto>>> GetSubdistrictsAsync();
        Task<Result<List<SubdistrictDto>>> GetSubdistrictsByDistrictIdAsync(int districtId);
        Task<Result<SubdistrictDto>> GetSubdistrictByIdAsync(int id);

        // Villages
        Task<Result<List<VillageDto>>> GetVillagesAsync();
        Task<Result<List<VillageDto>>> GetVillagesBySubdistrictIdAsync(int subdistrictId);
        Task<Result<VillageDto>> GetVillageByIdAsync(int id);
    }
}