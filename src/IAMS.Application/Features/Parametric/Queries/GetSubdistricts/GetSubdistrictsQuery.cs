using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetSubdistricts;

public record GetSubdistrictsQuery : IRequest<Result<List<SubdistrictDto>>>;

public record GetSubdistrictsByDistrictIdQuery(int DistrictId) : IRequest<Result<List<SubdistrictDto>>>;

public record GetSubdistrictByIdQuery(int Id) : IRequest<Result<SubdistrictDto>>;