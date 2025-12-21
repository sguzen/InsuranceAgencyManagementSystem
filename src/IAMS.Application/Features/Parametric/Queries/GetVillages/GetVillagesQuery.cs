using IAMS.Shared.DTOs.Parametric;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Parametric.Queries.GetVillages;

public record GetVillagesQuery : IRequest<Result<List<VillageDto>>>;

public record GetVillagesBySubdistrictIdQuery(int SubdistrictId) : IRequest<Result<List<VillageDto>>>;

public record GetVillageByIdQuery(int Id) : IRequest<Result<VillageDto>>;