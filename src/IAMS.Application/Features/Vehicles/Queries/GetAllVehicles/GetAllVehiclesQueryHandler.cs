using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
                // OPTIMIZED: Use IQueryable for database-level filtering and pagination
                var query = _unitOfWork.Vehicles.AsQueryable().Where(v => !v.IsDeleted);

                // Apply filters from query params at database level
                if (!string.IsNullOrEmpty(request.QueryParams.PlateNumber))
                {
                    query = query.Where(v => v.PlateNumber.Contains(request.QueryParams.PlateNumber));
                }

                if (!string.IsNullOrEmpty(request.QueryParams.ChassisNumber))
                {
                    query = query.Where(v => v.ChassisNumber.Contains(request.QueryParams.ChassisNumber));
                }

                if (request.QueryParams.CustomerId.HasValue)
                {
                    query = query.Where(v => v.CustomerId == request.QueryParams.CustomerId.Value);
                }

                if (request.QueryParams.BrandId.HasValue)
                {
                    query = query.Where(v => v.BrandId == request.QueryParams.BrandId.Value);
                }

                if (request.QueryParams.ModelId.HasValue)
                {
                    query = query.Where(v => v.ModelId == request.QueryParams.ModelId.Value);
                }

                if (request.QueryParams.ModelYear.HasValue)
                {
                    query = query.Where(v => v.ModelYear == request.QueryParams.ModelYear.Value);
                }

                // Get count at database level
                var totalCount = await query.CountAsync(cancellationToken);

                // Apply pagination at database level
                var pagedVehicles = await query
                    .OrderByDescending(v => v.CreatedOn)
                    .Skip((request.QueryParams.PageNumber - 1) * request.QueryParams.PageSize)
                    .Take(request.QueryParams.PageSize)
                    .ToListAsync(cancellationToken);

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
