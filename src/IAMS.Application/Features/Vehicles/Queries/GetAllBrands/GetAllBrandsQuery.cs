using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetAllBrands
{
    public class GetAllBrandsQuery : IRequest<Result<List<VehicleBrandDto>>>
    {
    }
}
