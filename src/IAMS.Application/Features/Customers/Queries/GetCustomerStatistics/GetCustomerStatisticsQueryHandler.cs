using IAMS.Application.DTOs.Customer;
using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Customers.Queries.GetCustomerStatistics
{
    public class GetCustomerStatisticsQueryHandler : IRequestHandler<GetCustomerStatisticsQuery, Result<CustomerStatisticsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetCustomerStatisticsQueryHandler> _logger;

        public GetCustomerStatisticsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<GetCustomerStatisticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<CustomerStatisticsDto>> Handle(GetCustomerStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var statistics = await _unitOfWork.Customers.GetCustomerStatisticsAsync();

                _logger.LogDebug("Retrieved customer statistics");

                return Result<CustomerStatisticsDto>.Success(statistics, "Müşteri istatistikleri getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics");
                return Result<CustomerStatisticsDto>.InternalError("Müşteri istatistikleri alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}