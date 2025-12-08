using IAMS.Application.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetOccupations;

public record GetOccupationsQuery : IRequest<Result<List<OccupationDto>>>;

public record GetOccupationByIdQuery(int Id) : IRequest<Result<OccupationDto>>;