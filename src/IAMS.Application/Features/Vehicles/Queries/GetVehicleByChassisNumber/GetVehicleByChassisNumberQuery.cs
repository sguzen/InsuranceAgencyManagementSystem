using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehicleByChassisNumber
{
    public class GetVehicleByChassisNumberQuery : IRequest<Result<VehicleDto>>
    {
        public string ChassisNumber { get; set; }

        public GetVehicleByChassisNumberQuery(string chassisNumber)
        {
            ChassisNumber = chassisNumber;
        }
    }
}
