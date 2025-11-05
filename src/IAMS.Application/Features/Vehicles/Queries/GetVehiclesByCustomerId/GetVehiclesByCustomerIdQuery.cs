using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
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
