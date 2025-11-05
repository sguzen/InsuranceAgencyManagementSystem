using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesByBrand
{
    public class GetVehiclesByBrandQueryHandler : IRequestHandler<GetVehiclesByBrandQuery, Result<Dictionary<string, int>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetVehiclesByBrandQueryHandler> _logger;

        public GetVehiclesByBrandQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetVehiclesByBrandQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Dictionary<string, int>>> Handle(GetVehiclesByBrandQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();

                var vehiclesByBrand = vehicles
                    .GroupBy(v => v.Brand.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                return Result<Dictionary<string, int>>.Success(vehiclesByBrand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles by brand statistics");
                return Result<Dictionary<string, int>>.InternalError("Error retrieving vehicles by brand", new List<string> { ex.Message });
            }
        }
    }
}
