using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue
{
    public class GetMonthlyRevenueQueryHandler : IRequestHandler<GetMonthlyRevenueQuery, Result<decimal>>
    {
        private readonly IPolicyAnalyticsService _policyAnalyticsService;
        private readonly ILogger<GetMonthlyRevenueQueryHandler> _logger;

        public GetMonthlyRevenueQueryHandler(
            IPolicyAnalyticsService policyAnalyticsService,
            ILogger<GetMonthlyRevenueQueryHandler> logger)
        {
            _policyAnalyticsService = policyAnalyticsService;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var revenue = await _policyAnalyticsService.GetMonthlyRevenueAsync();

                _logger.LogDebug("Retrieved monthly revenue {Revenue} for tenant {TenantId}", revenue);

                return Result<decimal>.Success(revenue, $"Bu ay toplam gelir: {revenue:C}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue");
                return Result<decimal>.InternalError("Aylık gelir alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}