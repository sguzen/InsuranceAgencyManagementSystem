using IAMS.Shared.DTOs.Parametric;
using IAMS.Application.Features.Parametric.Queries;
using IAMS.Application.Features.Parametric.Queries.GetCities;
using IAMS.Application.Features.Parametric.Queries.GetCountries;
using IAMS.Application.Features.Parametric.Queries.GetDistricts;
using IAMS.Application.Features.Parametric.Queries.GetOccupations;
using IAMS.Application.Features.Parametric.Queries.GetSubdistricts;
using IAMS.Application.Features.Parametric.Queries.GetVillages;
using IAMS.Application.Interfaces;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Services.Parametric;

public class ParametricService : IParametricService
{
    private readonly IMediator _mediator;

    public ParametricService(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Countries

    public async Task<Result<List<CountryDto>>> GetCountriesAsync()
    {
        return await _mediator.Send(new GetCountriesQuery());
    }

    public async Task<Result<CountryDto>> GetCountryByIdAsync(int id)
    {
        return await _mediator.Send(new GetCountryByIdQuery(id));
    }

    public async Task<Result<CountryDto>> GetCountryByCodeAsync(string code)
    {
        return await _mediator.Send(new GetCountryByCodeQuery(code));
    }

    #endregion

    #region Occupations

    public async Task<Result<List<OccupationDto>>> GetOccupationsAsync()
    {
        return await _mediator.Send(new GetOccupationsQuery());
    }

    public async Task<Result<OccupationDto>> GetOccupationByIdAsync(int id)
    {
        return await _mediator.Send(new GetOccupationByIdQuery(id));
    }

    #endregion

    #region Cities

    public async Task<Result<List<CityDto>>> GetCitiesAsync()
    {
        return await _mediator.Send(new GetCitiesQuery());
    }

    public async Task<Result<CityDto>> GetCityByIdAsync(int id)
    {
        return await _mediator.Send(new GetCityByIdQuery(id));
    }

    #endregion

    #region Districts

    public async Task<Result<List<DistrictDto>>> GetDistrictsAsync()
    {
        return await _mediator.Send(new GetDistrictsQuery());
    }

    public async Task<Result<List<DistrictDto>>> GetDistrictsByCityIdAsync(int cityId)
    {
        return await _mediator.Send(new GetDistrictsByCityIdQuery(cityId));
    }

    public async Task<Result<DistrictDto>> GetDistrictByIdAsync(int id)
    {
        return await _mediator.Send(new GetDistrictByIdQuery(id));
    }

    #endregion

    #region Subdistricts

    public async Task<Result<List<SubdistrictDto>>> GetSubdistrictsAsync()
    {
        return await _mediator.Send(new GetSubdistrictsQuery());
    }

    public async Task<Result<List<SubdistrictDto>>> GetSubdistrictsByDistrictIdAsync(int districtId)
    {
        return await _mediator.Send(new GetSubdistrictsByDistrictIdQuery(districtId));
    }

    public async Task<Result<SubdistrictDto>> GetSubdistrictByIdAsync(int id)
    {
        return await _mediator.Send(new GetSubdistrictByIdQuery(id));
    }

    #endregion

    #region Villages

    public async Task<Result<List<VillageDto>>> GetVillagesAsync()
    {
        return await _mediator.Send(new GetVillagesQuery());
    }

    public async Task<Result<List<VillageDto>>> GetVillagesBySubdistrictIdAsync(int subdistrictId)
    {
        return await _mediator.Send(new GetVillagesBySubdistrictIdQuery(subdistrictId));
    }

    public async Task<Result<VillageDto>> GetVillageByIdAsync(int id)
    {
        return await _mediator.Send(new GetVillageByIdQuery(id));
    }

    #endregion
}