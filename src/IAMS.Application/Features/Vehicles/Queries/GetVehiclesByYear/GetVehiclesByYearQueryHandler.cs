using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetVehiclesByYear
{
    public class GetVehiclesByYearQueryHandler : IRequestHandler<GetVehiclesByYearQuery, Result<Dictionary<int, int>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetVehiclesByYearQueryHandler> _logger;

        public GetVehiclesByYearQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetVehiclesByYearQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Dictionary<int, int>>> Handle(GetVehiclesByYearQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();

                var vehiclesByYear = vehicles
                    .Where(v => v.ModelYear.HasValue)
                    .GroupBy(v => v.ModelYear!.Value)
                    .ToDictionary(g => g.Key, g => g.Count());

                return Result<Dictionary<int, int>>.Success(vehiclesByYear);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles by year statistics");
                return Result<Dictionary<int, int>>.InternalError("Error retrieving vehicles by year", new List<string> { ex.Message });
            }
        }
    }
}
