using IAMS.Domain.Entities;

namespace IAMS.Application.Interfaces.Repositories
{
    public interface ICountryRepository : IRepository<Country>
    {
        Task<List<Country>> GetActiveCountriesAsync();
        Task<Country?> GetByCodeAsync(string code);
    }

    public interface IOccupationRepository : IRepository<Occupation>
    {
        Task<List<Occupation>> GetActiveOccupationsAsync();
        Task<Occupation?> GetByCodeAsync(string code);
    }

    public interface ICityRepository : IRepository<City>
    {
        Task<List<City>> GetActiveCitiesAsync();
        Task<City?> GetByCodeAsync(string code);
    }

    public interface IDistrictRepository : IRepository<District>
    {
        Task<List<District>> GetActiveDistrictsAsync();
        Task<List<District>> GetByCityIdAsync(int cityId);
        Task<District?> GetByCodeAsync(string code);
    }

    public interface ISubdistrictRepository : IRepository<Subdistrict>
    {
        Task<List<Subdistrict>> GetActiveSubdistrictsAsync();
        Task<List<Subdistrict>> GetByDistrictIdAsync(int districtId);
        Task<Subdistrict?> GetByCodeAsync(string code);
    }

    public interface IVillageRepository : IRepository<Village>
    {
        Task<List<Village>> GetActiveVillagesAsync();
        Task<List<Village>> GetBySubdistrictIdAsync(int subdistrictId);
        Task<Village?> GetByCodeAsync(string code);
    }
}