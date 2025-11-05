using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Commands.DeleteVehicle
{
    public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteVehicleCommandHandler> _logger;

        public DeleteVehicleCommandHandler(
            IUnitOfWork unitOfWork,
            ILogger<DeleteVehicleCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.Id);
                if (vehicle == null)
                {
                    return Result.NotFound($"Vehicle with ID {request.Id} not found");
                }

                // Check if vehicle has any active policies
                var policies = await _unitOfWork.Policies.GetByVehicleIdAsync(request.Id);
                if (policies != null && policies.Any())
                {
                    return Result.ValidationError("Cannot delete vehicle with existing policies");
                }

                _unitOfWork.Vehicles.Delete(vehicle);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Vehicle deleted successfully with ID: {VehicleId}", request.Id);

                return Result.Success("Vehicle deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle with ID: {VehicleId}", request.Id);
                return Result.InternalError("Error deleting vehicle", new List<string> { ex.Message });
            }
        }
    }
}
