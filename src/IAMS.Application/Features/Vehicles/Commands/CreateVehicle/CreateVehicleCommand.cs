using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Commands.CreateVehicle
{
    public class CreateVehicleCommand : IRequest<Result<VehicleDto>>
    {
        public CreateVehicleDto VehicleDto { get; set; }

        public CreateVehicleCommand(CreateVehicleDto vehicleDto)
        {
            VehicleDto = vehicleDto;
        }
    }
}
