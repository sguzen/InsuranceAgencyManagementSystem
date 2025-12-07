using AutoMapper;
using IAMS.Application.DTOs.Vehicle;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesByCustomerId
{
    public class GetVehiclesByCustomerIdQueryHandler : IRequestHandler<GetVehiclesByCustomerIdQuery, Result<List<VehicleDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetVehiclesByCustomerIdQueryHandler> _logger;

        public GetVehiclesByCustomerIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetVehiclesByCustomerIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<VehicleDto>>> Handle(GetVehiclesByCustomerIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetByCustomerIdAsync(request.CustomerId);
                var vehicleDtos = _mapper.Map<List<VehicleDto>>(vehicles);

                return Result<List<VehicleDto>>.Success(vehicleDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles for customer ID: {CustomerId}", request.CustomerId);
                return Result<List<VehicleDto>>.InternalError("Error retrieving vehicles", new List<string> { ex.Message });
            }
        }
    }
}
