using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetTotalVehiclesCount
{
    public class GetTotalVehiclesCountQuery : IRequest<Result<int>>
    {
    }
}
