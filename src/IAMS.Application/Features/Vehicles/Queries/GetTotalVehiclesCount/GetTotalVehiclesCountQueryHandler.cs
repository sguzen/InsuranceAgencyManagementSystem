using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Vehicles.Queries.GetTotalVehiclesCount
{
    public class GetTotalVehiclesCountQueryHandler : IRequestHandler<GetTotalVehiclesCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTotalVehiclesCountQueryHandler> _logger;

        public GetTotalVehiclesCountQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTotalVehiclesCountQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(GetTotalVehiclesCountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var vehicles = await _unitOfWork.Vehicles.GetAllAsync();
                var count = vehicles.Count();

                return Result<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total vehicles count");
                return Result<int>.InternalError("Error retrieving total vehicles count", new List<string> { ex.Message });
            }
        }
    }
}
