using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehicleByPlateNumber
{
    public class GetVehicleByPlateNumberQueryHandler : IRequestHandler<GetVehicleByPlateNumberQuery, Result<VehicleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVehicleByPlateNumberQueryHandler> _logger;

        public GetVehicleByPlateNumberQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVehicleByPlateNumberQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<VehicleDto>> Handle(GetVehicleByPlateNumberQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicle = await _unitOfWork.Vehicles.GetByPlateNumberAsync(request.PlateNumber);
                if (vehicle == null)
                {
                    return Result<VehicleDto>.NotFound($"Vehicle with plate number {request.PlateNumber} not found");
                }

                var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
                return Result<VehicleDto>.Success(vehicleDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicle with plate number: {PlateNumber}", request.PlateNumber);
                return Result<VehicleDto>.InternalError("Error retrieving vehicle", new List<string> { ex.Message });
            }
        }
    }
}
