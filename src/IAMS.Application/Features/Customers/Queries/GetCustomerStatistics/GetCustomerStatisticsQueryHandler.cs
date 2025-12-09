using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces;

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
            return Result<CustomerStatisticsDto>.InternalError("Müşteri istatistikleri alınırken beklenmeyen bir hata oluştu");
            //try
            //{
            //    var statistics = await _unitOfWork.Customers.GetCustomerStatisticsAsync();

                // Map from value object to DTO
                var dto = new CustomerStatisticsDto
                {
                    TotalCustomers = statistics.TotalCustomers,
                    ActiveCustomers = statistics.ActiveCustomers,
                    InactiveCustomers = statistics.InactiveCustomers,
                    NewCustomersThisMonth = statistics.NewCustomersThisMonth,
                    NewCustomersThisWeek = statistics.NewCustomersThisWeek,
                    CustomerGrowthPercentage = statistics.CustomerGrowthPercentage,
                    CustomersWithActivePolicies = statistics.CustomersWithActivePolicies,
                    CustomersWithoutPolicies = statistics.CustomersWithoutPolicies,
                    AverageCustomerAge = statistics.AverageCustomerAge,
                    CustomersByStatus = statistics.CustomersByStatus,
                    CustomersByGender = statistics.CustomersByGender
                };

                _logger.LogDebug("Retrieved customer statistics");

                return Result<CustomerStatisticsDto>.Success(dto, "Müşteri istatistikleri getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics");
                return Result<CustomerStatisticsDto>.InternalError("Müşteri istatistikleri alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}