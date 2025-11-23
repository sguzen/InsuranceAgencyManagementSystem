using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Commands.CreateVehicle
{
    public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Result<VehicleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateVehicleCommandHandler> _logger;

        public CreateVehicleCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateVehicleCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verify customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.VehicleDto.CustomerId);
                if (customer == null)
                {
                    return Result<VehicleDto>.NotFound($"Customer with ID {request.VehicleDto.CustomerId} not found");
                }

                // Verify brand exists
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(request.VehicleDto.BrandId);
                if (brand == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle brand with ID {request.VehicleDto.BrandId} not found");
                }

                // Verify model exists and belongs to brand
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(request.VehicleDto.ModelId);
                if (model == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle model with ID {request.VehicleDto.ModelId} not found");
                }

                if (model.BrandId != request.VehicleDto.BrandId)
                {
                    return Result<VehicleDto>.Failure("Model does not belong to the specified brand", string.Empty);
                }

                // Check if plate number already exists
                var existingVehicle = await _unitOfWork.Vehicles.GetByPlateNumberAsync(request.VehicleDto.PlateNumber);
                if (existingVehicle != null)
                {
                    return Result<VehicleDto>.Failure($"Vehicle with plate number {request.VehicleDto.PlateNumber} already exists", string.Empty);
                }

                // Check if chassis number already exists
                existingVehicle = await _unitOfWork.Vehicles.GetByChassisNumberAsync(request.VehicleDto.ChassisNumber);
                if (existingVehicle != null)
                {
                    return Result<VehicleDto>.Failure($"Vehicle with chassis number {request.VehicleDto.ChassisNumber} already exists", string.Empty);
                }

                // Look up currency ID from currency code
                var currency = await _unitOfWork.Currencies.GetByCodeAsync(request.VehicleDto.Currency);
                if (currency == null)
                {
                    // Default to TRY if currency not found
                    currency = await _unitOfWork.Currencies.GetByCodeAsync("TRY");
                    if (currency == null)
                    {
                        return Result<VehicleDto>.Failure("Default currency (TRY) not found in system", string.Empty);
                    }
                }

                var vehicle = _mapper.Map<Vehicle>(request.VehicleDto);
                vehicle.CurrencyId = currency.Id; // Set the currency ID
                vehicle.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.Vehicles.AddAsync(vehicle);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Vehicle created successfully with ID: {VehicleId}", vehicle.Id);

                // Retrieve the complete vehicle with navigation properties
                var createdVehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicle.Id);
                var vehicleDto = _mapper.Map<VehicleDto>(createdVehicle);

                return Result<VehicleDto>.Success(vehicleDto, "Vehicle created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle with plate number: {PlateNumber}", request.VehicleDto.PlateNumber);
                return Result<VehicleDto>.InternalError("Error creating vehicle", new List<string> { ex.Message });
            }
        }
    }
}
