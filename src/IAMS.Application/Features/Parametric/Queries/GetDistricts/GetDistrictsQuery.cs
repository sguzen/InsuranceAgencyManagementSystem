using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetDistricts;

public record GetDistrictsQuery : IRequest<Result<List<DistrictDto>>>;

public record GetDistrictsByCityIdQuery(int CityId) : IRequest<Result<List<DistrictDto>>>;

public record GetDistrictByIdQuery(int Id) : IRequest<Result<DistrictDto>>;