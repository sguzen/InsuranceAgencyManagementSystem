using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Commands.DeleteVehicle
{
    public class DeleteVehicleCommand : IRequest<Result>
    {
        public int Id { get; set; }

        public DeleteVehicleCommand(int id)
        {
            Id = id;
        }
    }
}
