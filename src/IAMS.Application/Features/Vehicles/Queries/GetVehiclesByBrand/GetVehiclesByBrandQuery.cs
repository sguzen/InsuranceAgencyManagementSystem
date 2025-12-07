using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesByBrand
{
    public class GetVehiclesByBrandQuery : IRequest<Result<Dictionary<string, int>>>
    {
    }
}
