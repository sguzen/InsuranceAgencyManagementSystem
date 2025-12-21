using IAMS.Shared.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetSubdistricts;

public record GetSubdistrictsQuery : IRequest<Result<List<SubdistrictDto>>>;

public record GetSubdistrictsByDistrictIdQuery(int DistrictId) : IRequest<Result<List<SubdistrictDto>>>;

public record GetSubdistrictByIdQuery(int Id) : IRequest<Result<SubdistrictDto>>;