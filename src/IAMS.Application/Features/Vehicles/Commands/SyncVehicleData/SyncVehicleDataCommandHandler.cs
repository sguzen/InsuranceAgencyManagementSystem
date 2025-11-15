using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Commands.SyncVehicleData
{
    /// <summary>
    /// Handler for synchronizing vehicle brands and models from external API
    /// </summary>
    public class SyncVehicleDataCommandHandler : IRequestHandler<SyncVehicleDataCommand, Result<VehicleDataSyncResult>>
    {
        private readonly IVehicleDataService _vehicleDataService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SyncVehicleDataCommandHandler> _logger;

        public SyncVehicleDataCommandHandler(
            IVehicleDataService vehicleDataService,
            IUnitOfWork unitOfWork,
            ILogger<SyncVehicleDataCommandHandler> logger)
        {
            _vehicleDataService = vehicleDataService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<VehicleDataSyncResult>> Handle(
            SyncVehicleDataCommand request,
            CancellationToken cancellationToken)
        {
            var result = new VehicleDataSyncResult { Success = true };

            try
            {
                _logger.LogInformation("Starting vehicle data synchronization from external API");

                // Fetch data from external API
                var externalData = await _vehicleDataService.FetchVehicleDataAsync();

                if (externalData == null || !externalData.Any())
                {
                    _logger.LogWarning("No vehicle data received from external API");
                    result.Warnings.Add("No data received from external API");
                    return Result<VehicleDataSyncResult>.Success(result, "No data to synchronize");
                }

                _logger.LogInformation("Fetched {Count} vehicle records from external API", externalData.Count);

                // Get existing brands and models
                var existingBrands = await _unitOfWork.VehicleBrands.GetAllAsync();
                var existingModels = await _unitOfWork.VehicleModels.GetAllAsync();

                // Group external data by brand
                var brandGroups = externalData
                    .GroupBy(x => new { x.BrandCode, x.BrandName })
                    .OrderBy(g => g.Key.BrandName);

                foreach (var brandGroup in brandGroups)
                {
                    try
                    {
                        // Process brand
                        var brandCode = brandGroup.Key.BrandCode.ToString();
                        var brandName = brandGroup.Key.BrandName;

                        var existingBrand = existingBrands.FirstOrDefault(b =>
                            b.Code == brandCode ||
                            b.Name.Equals(brandName, StringComparison.OrdinalIgnoreCase));

                        if (existingBrand == null)
                        {
                            // Create new brand
                            var newBrand = new VehicleBrand
                            {
                                Name = brandName,
                                Code = brandCode,
                                IsActive = true,
                                DisplayOrder = result.BrandsCreated + result.BrandsUpdated
                            };

                            await _unitOfWork.VehicleBrands.AddAsync(newBrand);
                            await _unitOfWork.SaveChangesAsync();

                            existingBrand = newBrand;
                            result.BrandsCreated++;
                            _logger.LogDebug("Created new brand: {BrandName} (Code: {BrandCode})", brandName, brandCode);
                        }
                        else if (request.UpdateExisting)
                        {
                            // Update existing brand
                            var brandUpdated = false;

                            if (existingBrand.Name != brandName)
                            {
                                existingBrand.Name = brandName;
                                brandUpdated = true;
                            }

                            if (existingBrand.Code != brandCode)
                            {
                                existingBrand.Code = brandCode;
                                brandUpdated = true;
                            }

                            if (!existingBrand.IsActive)
                            {
                                existingBrand.IsActive = true;
                                brandUpdated = true;
                            }

                            if (brandUpdated)
                            {
                                await _unitOfWork.VehicleBrands.UpdateAsync(existingBrand);
                                result.BrandsUpdated++;
                                _logger.LogDebug("Updated brand: {BrandName} (Code: {BrandCode})", brandName, brandCode);
                            }
                        }

                        // Process models for this brand
                        foreach (var modelData in brandGroup)
                        {
                            try
                            {
                                var modelName = modelData.ModelName;
                                if (string.IsNullOrWhiteSpace(modelName))
                                    continue;

                                var existingModel = existingModels.FirstOrDefault(m =>
                                    m.BrandId == existingBrand.Id &&
                                    m.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase));

                                if (existingModel == null)
                                {
                                    // Create new model
                                    var newModel = new VehicleModel
                                    {
                                        BrandId = existingBrand.Id,
                                        Name = modelName,
                                        Code = modelData.Id.ToString(),
                                        IsActive = true,
                                        DisplayOrder = result.ModelsCreated + result.ModelsUpdated
                                    };

                                    await _unitOfWork.VehicleModels.AddAsync(newModel);
                                    result.ModelsCreated++;
                                    _logger.LogDebug(
                                        "Created new model: {ModelName} for brand {BrandName}",
                                        modelName,
                                        brandName);
                                }
                                else if (request.UpdateExisting)
                                {
                                    // Update existing model
                                    var modelUpdated = false;

                                    if (existingModel.Name != modelName)
                                    {
                                        existingModel.Name = modelName;
                                        modelUpdated = true;
                                    }

                                    if (!existingModel.IsActive)
                                    {
                                        existingModel.IsActive = true;
                                        modelUpdated = true;
                                    }

                                    if (modelUpdated)
                                    {
                                        await _unitOfWork.VehicleModels.UpdateAsync(existingModel);
                                        result.ModelsUpdated++;
                                        _logger.LogDebug(
                                            "Updated model: {ModelName} for brand {BrandName}",
                                            modelName,
                                            brandName);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(
                                    ex,
                                    "Error processing model {ModelName} for brand {BrandName}",
                                    modelData.ModelName,
                                    brandName);
                                result.Warnings.Add($"Failed to process model: {modelData.ModelName}");
                            }
                        }

                        // Save changes after each brand to avoid losing all progress on error
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Error processing brand {BrandName} (Code: {BrandCode})",
                            brandGroup.Key.BrandName,
                            brandGroup.Key.BrandCode);
                        result.Warnings.Add($"Failed to process brand: {brandGroup.Key.BrandName}");
                    }
                }

                // Handle deactivation of missing items if requested
                if (request.DeactivateMissing)
                {
                    var externalBrandCodes = externalData
                        .Select(x => x.BrandCode.ToString())
                        .Distinct()
                        .ToHashSet();

                    foreach (var brand in existingBrands.Where(b => b.IsActive && !externalBrandCodes.Contains(b.Code)))
                    {
                        brand.IsActive = false;
                        await _unitOfWork.VehicleBrands.UpdateAsync(brand);
                        result.Warnings.Add($"Deactivated brand not found in external data: {brand.Name}");
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation(
                    "Vehicle data synchronization completed. Brands: {BrandsCreated} created, {BrandsUpdated} updated. Models: {ModelsCreated} created, {ModelsUpdated} updated",
                    result.BrandsCreated,
                    result.BrandsUpdated,
                    result.ModelsCreated,
                    result.ModelsUpdated);

                var message = $"Synchronization completed: {result.BrandsCreated} brands created, {result.BrandsUpdated} brands updated, {result.ModelsCreated} models created, {result.ModelsUpdated} models updated";
                return Result<VehicleDataSyncResult>.Success(result, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during vehicle data synchronization");
                result.Success = false;
                result.ErrorMessage = $"Synchronization failed: {ex.Message}";
                return Result<VehicleDataSyncResult>.Failure(result.ErrorMessage);
            }
        }
    }
}
