using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehicleByChassisNumber
{
    public class GetVehicleByChassisNumberQueryHandler : IRequestHandler<GetVehicleByChassisNumberQuery, Result<VehicleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVehicleByChassisNumberQueryHandler> _logger;

        public GetVehicleByChassisNumberQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVehicleByChassisNumberQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> Handle(GetVehicleByChassisNumberQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByChassisNumberAsync(request.ChassisNumber);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with chassis number {request.ChassisNumber} not found");
                }

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with chassis number: {ChassisNumber}", request.ChassisNumber);
                return Result<VehicleDto>.InternalError("Error retrieving vehicle", new List<string> { ex.Message });
            }
        }
    }
}
