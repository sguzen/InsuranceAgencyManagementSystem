using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesDueForInspection
{
    public class GetVehiclesDueForInspectionQueryHandler : IRequestHandler<GetVehiclesDueForInspectionQuery, Result<List<VehicleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVehiclesDueForInspectionQueryHandler> _logger;

        public GetVehiclesDueForInspectionQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVehiclesDueForInspectionQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VehicleDto>>> Handle(GetVehiclesDueForInspectionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetVehiclesDueForInspectionAsync(request.DaysAhead);
                var vehicleDtos = _mapper.Map<List<VehicleDto>>(vehicles);

                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles due for inspection within {DaysAhead} days", request.DaysAhead);
                return Result<List<VehicleDto>>.InternalError("Error retrieving vehicles due for inspection", new List<string> { ex.Message });
            }
        }
    }
}
