using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Features.Vehicles.Commands.UpdateInspectionDates
{
    public class UpdateInspectionDatesCommand : IRequest<Result>
    {
        public int VehicleId { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }

        public UpdateInspectionDatesCommand(int vehicleId, DateTime? lastInspectionDate, DateTime? nextInspectionDate)
        {
            VehicleId = vehicleId;
            LastInspectionDate = lastInspectionDate;
            NextInspectionDate = nextInspectionDate;
        }
    }
}
