using IAMS.Application.DTOs.Parametric;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetCountries;

public record GetCountriesQuery : IRequest<Result<List<CountryDto>>>;

public record GetCountryByIdQuery(int Id) : IRequest<Result<CountryDto>>;

public record GetCountryByCodeQuery(string Code) : IRequest<Result<CountryDto>>;