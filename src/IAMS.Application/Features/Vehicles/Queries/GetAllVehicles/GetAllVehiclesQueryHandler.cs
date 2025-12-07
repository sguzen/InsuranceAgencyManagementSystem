using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetAllVehicles
{
    public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, Result<PagedResult<VehicleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetAllVehiclesQueryHandler> _logger;

        public GetAllVehiclesQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetAllVehiclesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PagedResult<VehicleDto>>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();

                // Apply filters from query params
                var filteredVehicles = vehicles.AsQueryable();

                if (!string.IsNullOrEmpty(request.QueryParams.PlateNumber))
                {
                    filteredVehicles = filteredVehicles.Where(v =>
                        v.PlateNumber.Contains(request.QueryParams.PlateNumber, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(request.QueryParams.ChassisNumber))
                {
                    filteredVehicles = filteredVehicles.Where(v =>
                        v.ChassisNumber.Contains(request.QueryParams.ChassisNumber, StringComparison.OrdinalIgnoreCase));
                }

                if (request.QueryParams.CustomerId.HasValue)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.CustomerId == request.QueryParams.CustomerId.Value);
                }

                if (request.QueryParams.BrandId.HasValue)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.BrandId == request.QueryParams.BrandId.Value);
                }

                if (request.QueryParams.ModelId.HasValue)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.ModelId == request.QueryParams.ModelId.Value);
                }

                if (request.QueryParams.ModelYear.HasValue)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.ModelYear == request.QueryParams.ModelYear.Value);
                }

                var totalCount = filteredVehicles.Count();

                // Apply pagination
                var pagedVehicles = filteredVehicles
                    .Skip((request.QueryParams.PageNumber - 1) * request.QueryParams.PageSize)
                    .Take(request.QueryParams.PageSize)
                    .ToList();

                var vehicleDtos = _mapper.Map<List<VehicleDto>>(pagedVehicles);

                var pagedResult = new PagedResult<VehicleDto>
                {
                    Items = vehicleDtos,
                    TotalCount = totalCount,
                    PageNumber = request.QueryParams.PageNumber,
                    PageSize = request.QueryParams.PageSize
                };

                return Result<PagedResult<VehicleDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles with query params");
                return Result<PagedResult<VehicleDto>>.InternalError("Error retrieving vehicles", new List<string> { ex.Message });
            }
        }
    }
}
