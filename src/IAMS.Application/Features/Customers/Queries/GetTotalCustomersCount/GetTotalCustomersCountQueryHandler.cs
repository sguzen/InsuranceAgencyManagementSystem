using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetTotalCustomersCount
{
    public class GetTotalCustomersCountQueryHandler : IRequestHandler<GetTotalCustomersCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetTotalCustomersCountQueryHandler> _logger;

        public GetTotalCustomersCountQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<GetTotalCustomersCountQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(GetTotalCustomersCountQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var count = await _unitOfWork.Customers.GetCustomerCountAsync();
                _logger.LogDebug("Retrieved customer count {Count}", count );

                return Result<int>.Success(count, $"Toplam {count} müşteri");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer count}" );
                return Result<int>.InternalError("Müşteri sayısı alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}