using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Application.Services.Vehicles;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetAllVehicles
{
    public class GetAllVehiclesQuery : IRequest<Result<PagedResult<VehicleDto>>>
    {
        public VehicleQueryParams QueryParams { get; set; }

        public GetAllVehiclesQuery(VehicleQueryParams queryParams)
        {
            QueryParams = queryParams;
        }
    }
}
