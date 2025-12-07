using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesByCustomerId
{
    public class GetVehiclesByCustomerIdQuery : IRequest<Result<List<VehicleDto>>>
    {
        public int CustomerId { get; set; }

        public GetVehiclesByCustomerIdQuery(int customerId)
        {
            CustomerId = customerId;
        }
    }
}
