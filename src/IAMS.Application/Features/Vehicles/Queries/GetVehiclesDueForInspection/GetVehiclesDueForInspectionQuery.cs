using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesDueForInspection
{
    public class GetVehiclesDueForInspectionQuery : IRequest<Result<List<VehicleDto>>>
    {
        public int DaysAhead { get; set; }

        public GetVehiclesDueForInspectionQuery(int daysAhead = 30)
        {
            DaysAhead = daysAhead;
        }
    }
}
