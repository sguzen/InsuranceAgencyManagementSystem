using IAMS.Shared.DTOs.Vehicle;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehicleById
{
    public class GetVehicleByIdQuery : IRequest<Result<VehicleDto>>
    {
        public int Id { get; set; }

        public GetVehicleByIdQuery(int id)
        {
            Id = id;
        }
    }
}
