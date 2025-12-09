using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Commands.UpdateVehicle
{
    public class UpdateVehicleCommand : IRequest<Result<VehicleDto>>
    {
        public int Id { get; set; }
        public UpdateVehicleDto VehicleDto { get; set; }

        public UpdateVehicleCommand(int id, UpdateVehicleDto vehicleDto)
        {
            Id = id;
            VehicleDto = vehicleDto;
        }
    }
}
