using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetTotalCustomersCount
{
    public class GetTotalCustomersCountQueryHandler : IRequestHandler<GetTotalCustomersCountQuery, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetTotalCustomersCountQueryHandler> _logger;

        public GetTotalCustomersCountQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetTotalCustomersCountQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<int>> Handle(GetTotalCustomersCountQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var count = await _unitOfWork.Customers.GetCustomerCountAsync(request.TenantId);
                _logger.LogDebug("Retrieved customer count {Count} for tenant {TenantId}", count, request.TenantId);

                return Result<int>.Success(count, $"Toplam {count} müşteri");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer count for tenant {TenantId}", request.TenantId);
                return Result<int>.InternalError("Müşteri sayısı alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}