using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Services.Vehicles
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleService> _logger;

        public VehicleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<VehicleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        #region Vehicle CRUD

        public async Task<Result<VehicleDto>> GetByIdAsync(int id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with ID {id} not found");
                }

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with ID: {VehicleId}", id);
                return Result<VehicleDto>.InternalError("Error retrieving vehicle", new List<string> { ex.Message });
            }
        }

        public async Task<Result<VehicleDto>> GetByPlateNumberAsync(string plateNumber)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByPlateNumberAsync(plateNumber);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with plate number {plateNumber} not found");
                }

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with plate number: {PlateNumber}", plateNumber);
                return Result<VehicleDto>.InternalError("Error retrieving vehicle", new List<string> { ex.Message });
            }
        }

        public async Task<Result<VehicleDto>> GetByChassisNumberAsync(string chassisNumber)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByChassisNumberAsync(chassisNumber);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with chassis number {chassisNumber} not found");
                }

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with chassis number: {ChassisNumber}", chassisNumber);
                return Result<VehicleDto>.InternalError("Error retrieving vehicle", new List<string> { ex.Message });
            }
        }

        public async Task<Result<PagedResult<VehicleDto>>> GetAllAsync(VehicleQueryParams queryParams)
        {
            try
            {
                var query = (await _unitOfWork.Vehicles.GetAllAsync()).AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(queryParams.PlateNumber))
                {
                    query = query.Where(v => v.PlateNumber.Contains(queryParams.PlateNumber));
                }

                if (!string.IsNullOrWhiteSpace(queryParams.ChassisNumber))
                {
                    query = query.Where(v => v.ChassisNumber.Contains(queryParams.ChassisNumber));
                }

                if (queryParams.CustomerId.HasValue)
                {
                    query = query.Where(v => v.CustomerId == queryParams.CustomerId.Value);
                }

                if (queryParams.BrandId.HasValue)
                {
                    query = query.Where(v => v.BrandId == queryParams.BrandId.Value);
                }

                if (queryParams.ModelId.HasValue)
                {
                    query = query.Where(v => v.ModelId == queryParams.ModelId.Value);
                }

                if (queryParams.ModelYear.HasValue)
                {
                    query = query.Where(v => v.ModelYear == queryParams.ModelYear.Value);
                }

                var totalCount = query.Count();
                var vehicles = query
                    .OrderByDescending(v => v.CreatedOn)
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToList();

                var vehicleDtos = _mapper.Map<List<VehicleDto>>(vehicles);
                var pagedResult = PagedResult<VehicleDto>.Success(vehicleDtos, totalCount, queryParams.PageNumber, queryParams.PageSize);

                return Result<PagedResult<VehicleDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles");
                return Result<PagedResult<VehicleDto>>.InternalError("Error retrieving vehicles", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<VehicleDto>>> GetByCustomerIdAsync(int customerId)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetByCustomerIdAsync(customerId);
                var vehicleDtos = _mapper.Map<List<VehicleDto>>(vehicles);
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles for customer ID: {CustomerId}", customerId);
                return Result<List<VehicleDto>>.InternalError("Error retrieving vehicles", new List<string> { ex.Message });
            }
        }

        public async Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto dto)
        {
            try
            {
                // Validate customer exists
                var customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId);
                if (customer == null)
                {
                    return Result<VehicleDto>.NotFound($"Customer with ID {dto.CustomerId} not found");
                }

                // Validate brand exists
                var brand = await _unitOfWork.VehicleBrands.GetByIdAsync(dto.BrandId);
                if (brand == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle brand with ID {dto.BrandId} not found");
                }

                // Validate model exists
                var model = await _unitOfWork.VehicleModels.GetByIdAsync(dto.ModelId);
                if (model == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle model with ID {dto.ModelId} not found");
                }

                // Check if plate number already exists
                if (await _unitOfWork.Vehicles.PlateNumberExistsAsync(dto.PlateNumber))
                {
                    return Result<VehicleDto>.Conflict("A vehicle with this plate number already exists");
                }

                // Check if chassis number already exists
                if (await _unitOfWork.Vehicles.ChassisNumberExistsAsync(dto.ChassisNumber))
                {
                    return Result<VehicleDto>.Conflict("A vehicle with this chassis number already exists");
                }

                var vehicle = _mapper.Map<Vehicle>(dto);
                vehicle.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.Vehicles.AddAsync(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehicle created successfully with ID: {VehicleId}", vehicle.Id);

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto, "Vehicle created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return Result<VehicleDto>.InternalError("Error creating vehicle", new List<string> { ex.Message });
            }
        }

        public async Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto dto)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with ID {id} not found");
                }

                // Check if plate number already exists for another vehicle
                if (!string.IsNullOrEmpty(dto.PlateNumber) && dto.PlateNumber != vehicle.PlateNumber)
                {
                    if (await _unitOfWork.Vehicles.PlateNumberExistsAsync(dto.PlateNumber, id))
                    {
                        return Result<VehicleDto>.Conflict("A vehicle with this plate number already exists");
                    }
                }

                // Check if chassis number already exists for another vehicle
                if (!string.IsNullOrEmpty(dto.ChassisNumber) && dto.ChassisNumber != vehicle.ChassisNumber)
                {
                    if (await _unitOfWork.Vehicles.ChassisNumberExistsAsync(dto.ChassisNumber, id))
                    {
                        return Result<VehicleDto>.Conflict("A vehicle with this chassis number already exists");
                    }
                }

                _mapper.Map(dto, vehicle);
                vehicle.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Vehicles.Update(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehicle updated successfully with ID: {VehicleId}", id);

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto, "Vehicle updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle with ID: {VehicleId}", id);
                return Result<VehicleDto>.InternalError("Error updating vehicle", new List<string> { ex.Message });
            }
        }

        public async Task<Result> DeleteAsync(int id)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return Result.NotFound($"Vehicle with ID {id} not found");
                }

                _unitOfWork.Vehicles.Remove(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vehicle deleted successfully with ID: {VehicleId}", id);
                return Result.Success("Vehicle deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle with ID: {VehicleId}", id);
                return Result.InternalError("Error deleting vehicle", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Vehicle Brands & Models

        public async Task<Result<List<VehicleBrandDto>>> GetAllBrandsAsync()
        {
            try
            {
                var brands = await _unitOfWork.VehicleBrands.GetAllAsync();
                var brandDtos = _mapper.Map<List<VehicleBrandDto>>(brands);
                return Result<List<VehicleBrandDto>>.Success(brandDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle brands");
                return Result<List<VehicleBrandDto>>.InternalError("Error retrieving vehicle brands", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<VehicleBrandDto>>> GetActiveBrandsAsync()
        {
            try
            {
                var brands = await _unitOfWork.VehicleBrands.GetActiveBrandsAsync();
                var brandDtos = _mapper.Map<List<VehicleBrandDto>>(brands);
                return Result<List<VehicleBrandDto>>.Success(brandDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active vehicle brands");
                return Result<List<VehicleBrandDto>>.InternalError("Error retrieving active vehicle brands", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<VehicleModelDto>>> GetModelsByBrandIdAsync(int brandId)
        {
            try
            {
                var models = await _unitOfWork.VehicleModels.GetByBrandIdAsync(brandId);
                var modelDtos = _mapper.Map<List<VehicleModelDto>>(models);
                return Result<List<VehicleModelDto>>.Success(modelDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle models for brand ID: {BrandId}", brandId);
                return Result<List<VehicleModelDto>>.InternalError("Error retrieving vehicle models", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<VehicleModelDto>>> GetActiveModelsByBrandIdAsync(int brandId)
        {
            try
            {
                var models = await _unitOfWork.VehicleModels.GetActiveByBrandIdAsync(brandId);
                var modelDtos = _mapper.Map<List<VehicleModelDto>>(models);
                return Result<List<VehicleModelDto>>.Success(modelDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active vehicle models for brand ID: {BrandId}", brandId);
                return Result<List<VehicleModelDto>>.InternalError("Error retrieving active vehicle models", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Inspection Management

        public async Task<Result<List<VehicleDto>>> GetVehiclesDueForInspectionAsync(int daysAhead = 30)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetVehiclesDueForInspectionAsync(daysAhead);
                var vehicleDtos = _mapper.Map<List<VehicleDto>>(vehicles);
                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles due for inspection");
                return Result<List<VehicleDto>>.InternalError("Error retrieving vehicles due for inspection", new List<string> { ex.Message });
            }
        }

        public async Task<Result> UpdateInspectionDatesAsync(int vehicleId, DateTime? lastInspection, DateTime? nextInspection)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return Result.NotFound($"Vehicle with ID {vehicleId} not found");
                }

                if (lastInspection.HasValue)
                {
                    vehicle.LastInspectionDate = lastInspection.Value;
                }

                if (nextInspection.HasValue)
                {
                    vehicle.NextInspectionDate = nextInspection.Value;
                }

                vehicle.ModifiedOn = DateTime.UtcNow;

                _unitOfWork.Vehicles.Update(vehicle);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Inspection dates updated for vehicle ID: {VehicleId}", vehicleId);
                return Result.Success("Inspection dates updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating inspection dates for vehicle ID: {VehicleId}", vehicleId);
                return Result.InternalError("Error updating inspection dates", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Statistics

        public async Task<Result<int>> GetTotalVehiclesCountAsync()
        {
            try
            {
                var count = await _unitOfWork.Vehicles.CountAsync();
                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total vehicles count");
                return Result<int>.InternalError("Error getting total vehicles count", new List<string> { ex.Message });
            }
        }

        public async Task<Result<Dictionary<string, int>>> GetVehiclesByBrandAsync()
        {
            try
            {
                var result = await _unitOfWork.Vehicles.GetVehicleCountByBrandAsync();
                return Result<Dictionary<string, int>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by brand");
                return Result<Dictionary<string, int>>.InternalError("Error getting vehicles by brand", new List<string> { ex.Message });
            }
        }

        public async Task<Result<Dictionary<int, int>>> GetVehiclesByYearAsync()
        {
            try
            {
                var result = await _unitOfWork.Vehicles.GetVehicleCountByYearAsync();
                return Result<Dictionary<int, int>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles by year");
                return Result<Dictionary<int, int>>.InternalError("Error getting vehicles by year", new List<string> { ex.Message });
            }
        }

        #endregion
    }
}
