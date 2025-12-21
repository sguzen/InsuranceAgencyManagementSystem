using IAMS.Shared.DTOs.Vehicle;
using IAMS.Application.Features.Vehicles.Commands.CreateVehicle;
using IAMS.Application.Features.Vehicles.Commands.DeleteVehicle;
using IAMS.Application.Features.Vehicles.Commands.UpdateInspectionDates;
using IAMS.Application.Features.Vehicles.Commands.UpdateVehicle;
using IAMS.Application.Features.Vehicles.Queries.GetActiveBrands;
using IAMS.Application.Features.Vehicles.Queries.GetActiveModelsByBrandId;
using IAMS.Application.Features.Vehicles.Queries.GetAllBrands;
using IAMS.Application.Features.Vehicles.Queries.GetAllVehicles;
using IAMS.Application.Features.Vehicles.Queries.GetModelsByBrandId;
using IAMS.Application.Features.Vehicles.Queries.GetTotalVehiclesCount;
using IAMS.Application.Features.Vehicles.Queries.GetVehicleByChassisNumber;
using IAMS.Application.Features.Vehicles.Queries.GetVehicleById;
using IAMS.Application.Features.Vehicles.Queries.GetVehicleByPlateNumber;
using IAMS.Application.Features.Vehicles.Queries.GetVehiclesByBrand;
using IAMS.Application.Features.Vehicles.Queries.GetVehiclesByCustomerId;
using IAMS.Application.Features.Vehicles.Queries.GetVehiclesByYear;
using IAMS.Application.Features.Vehicles.Queries.GetVehiclesDueForInspection;
using IAMS.Shared.Models;
using MediatR;

namespace IAMS.Application.Services.Vehicles
{
    public class VehicleService : IVehicleService
    {
        private readonly IMediator _mediator;

        public VehicleService(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Vehicle CRUD
        public async Task<Result<VehicleDto>> GetByIdAsync(int id)
        {
            return await _mediator.Send(new GetVehicleByIdQuery(id));
        }

        public async Task<Result<VehicleDto>> GetByPlateNumberAsync(string plateNumber)
        {
            return await _mediator.Send(new GetVehicleByPlateNumberQuery(plateNumber));
        }

        public async Task<Result<VehicleDto>> GetByChassisNumberAsync(string chassisNumber)
        {
            return await _mediator.Send(new GetVehicleByChassisNumberQuery(chassisNumber));
        }

        public async Task<Result<PagedResult<VehicleDto>>> GetAllAsync(VehicleQueryParams queryParams)
        {
            return await _mediator.Send(new GetAllVehiclesQuery(queryParams));
        }

        public async Task<Result<List<VehicleDto>>> GetByCustomerIdAsync(int customerId)
        {
            return await _mediator.Send(new GetVehiclesByCustomerIdQuery(customerId));
        }

        public async Task<Result<VehicleDto>> CreateAsync(CreateVehicleDto dto)
        {
            return await _mediator.Send(new CreateVehicleCommand(dto));
        }

        public async Task<Result<VehicleDto>> UpdateAsync(int id, UpdateVehicleDto dto)
        {
            return await _mediator.Send(new UpdateVehicleCommand(id, dto));
        }

        public async Task<Result> DeleteAsync(int id)
        {
            return await _mediator.Send(new DeleteVehicleCommand(id));
        }

        // Vehicle Brands & Models
        public async Task<Result<List<VehicleBrandDto>>> GetAllBrandsAsync()
        {
            return await _mediator.Send(new GetAllBrandsQuery());
        }

        public async Task<Result<List<VehicleBrandDto>>> GetActiveBrandsAsync()
        {
            return await _mediator.Send(new GetActiveBrandsQuery());
        }

        public async Task<Result<List<VehicleModelDto>>> GetModelsByBrandIdAsync(int brandId)
        {
            return await _mediator.Send(new GetModelsByBrandIdQuery(brandId));
        }

        public async Task<Result<List<VehicleModelDto>>> GetActiveModelsByBrandIdAsync(int brandId)
        {
            return await _mediator.Send(new GetActiveModelsByBrandIdQuery(brandId));
        }

        // Inspection Management
        public async Task<Result<List<VehicleDto>>> GetVehiclesDueForInspectionAsync(int daysAhead = 30)
        {
            return await _mediator.Send(new GetVehiclesDueForInspectionQuery(daysAhead));
        }

        public async Task<Result> UpdateInspectionDatesAsync(int vehicleId, DateTime? lastInspection, DateTime? nextInspection)
        {
            return await _mediator.Send(new UpdateInspectionDatesCommand(vehicleId, lastInspection, nextInspection));
        }

        // Statistics
        public async Task<Result<int>> GetTotalVehiclesCountAsync()
        {
            return await _mediator.Send(new GetTotalVehiclesCountQuery());
        }

        public async Task<Result<Dictionary<string, int>>> GetVehiclesByBrandAsync()
        {
            return await _mediator.Send(new GetVehiclesByBrandQuery());
        }

        public async Task<Result<Dictionary<int, int>>> GetVehiclesByYearAsync()
        {
            return await _mediator.Send(new GetVehiclesByYearQuery());
        }
    }
}
