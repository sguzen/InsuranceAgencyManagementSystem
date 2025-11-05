using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Commands.UpdateInspectionDates
{
    public class UpdateInspectionDatesCommandHandler : IRequestHandler<UpdateInspectionDatesCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateInspectionDatesCommandHandler> _logger;

        public UpdateInspectionDatesCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<UpdateInspectionDatesCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateInspectionDatesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.VehicleId);
                if (vehicle == null)
                {
                    return Result.NotFound($"Vehicle with ID {request.VehicleId} not found");
                }

                if (request.LastInspectionDate.HasValue)
                {
                    vehicle.LastInspectionDate = request.LastInspectionDate;
                }

                if (request.NextInspectionDate.HasValue)
                {
                    vehicle.NextInspectionDate = request.NextInspectionDate;
                }

                vehicle.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Vehicles.Update(vehicle);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Inspection dates updated successfully for vehicle ID: {VehicleId}", request.VehicleId);

                return Result.Success("Inspection dates updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inspection dates for vehicle ID: {VehicleId}", request.VehicleId);
                return Result.InternalError("Error updating inspection dates", new List<string> { ex.Message });
            }
        }
    }
}
