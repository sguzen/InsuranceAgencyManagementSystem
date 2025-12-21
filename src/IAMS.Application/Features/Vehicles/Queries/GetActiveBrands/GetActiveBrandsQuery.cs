using IAMS.Shared.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetActiveBrands
{
    public class GetActiveBrandsQuery : IRequest<Result<List<VehicleBrandDto>>>
    {
    }
}
