using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesByYear
{
    public class GetVehiclesByYearQuery : IRequest<Result<Dictionary<int, int>>>
    {
    }
}
