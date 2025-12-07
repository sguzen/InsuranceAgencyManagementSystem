using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetAllBrands
{
    public class GetAllBrandsQuery : IRequest<Result<List<VehicleBrandDto>>>
    {
    }
}
