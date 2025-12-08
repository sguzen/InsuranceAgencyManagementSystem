using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Commands.UpdateVehicle
{
    public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Result<VehicleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateVehicleCommandHandler> _logger;

        public UpdateVehicleCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateVehicleCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(request.Id);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with ID {request.Id} not found");
                }

                // Update only the fields present in UpdateVehicleDto
                if (request.VehicleDto.Color != null)
                    vehicle.Color = request.VehicleDto.Color;

                if (request.VehicleDto.CurrentValue.HasValue)
                    vehicle.CurrentValue = request.VehicleDto.CurrentValue;

                if (request.VehicleDto.LastInspectionDate.HasValue)
                    vehicle.LastInspectionDate = request.VehicleDto.LastInspectionDate;

                if (request.VehicleDto.NextInspectionDate.HasValue)
                    vehicle.NextInspectionDate = request.VehicleDto.NextInspectionDate;

                if (request.VehicleDto.Notes != null)
                    vehicle.Notes = request.VehicleDto.Notes;

                vehicle.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Vehicles.Update(vehicle);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Vehicle updated successfully with ID: {VehicleId}", vehicle.Id);

                // Retrieve the updated vehicle with navigation properties
                var updatedVehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicle.Id);
                var vehicleDto = _mapper.Map<VehicleDto>(updatedVehicle);

                return Result<VehicleDto>.Success(vehicleDto, "Vehicle updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle with ID: {VehicleId}", request.Id);
                return Result<VehicleDto>.InternalError("Error updating vehicle", new List<string> { ex.Message });
            }
        }
    }
}
