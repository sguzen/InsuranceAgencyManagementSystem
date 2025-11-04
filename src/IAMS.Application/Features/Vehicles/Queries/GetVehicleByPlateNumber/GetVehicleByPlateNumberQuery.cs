using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehicleByPlateNumber
{
    public class GetVehicleByPlateNumberQuery : IRequest<Result<VehicleDto>>
    {
        public string PlateNumber { get; set; }

        public GetVehicleByPlateNumberQuery(string plateNumber)
        {
            PlateNumber = plateNumber;
        }
    }
}
