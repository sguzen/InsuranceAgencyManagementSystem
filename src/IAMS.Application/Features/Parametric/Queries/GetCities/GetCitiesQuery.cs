using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetCities;

public record GetCitiesQuery : IRequest<Result<List<CityDto>>>;

public record GetCityByIdQuery(int Id) : IRequest<Result<CityDto>>;