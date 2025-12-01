using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenueByCurrency
{
    public class GetMonthlyRevenueByCurrencyQueryHandler : IRequestHandler<GetMonthlyRevenueByCurrencyQuery, Result<Dictionary<string, decimal>>>
    {
        private readonly IPolicyAnalyticsService _policyAnalyticsService;
        private readonly ILogger<GetMonthlyRevenueByCurrencyQueryHandler> _logger;

        public GetMonthlyRevenueByCurrencyQueryHandler(
            IPolicyAnalyticsService policyAnalyticsService,
            ILogger<GetMonthlyRevenueByCurrencyQueryHandler> logger)
        {
            _policyAnalyticsService = policyAnalyticsService;
            _logger = logger;
        }

        public async Task<Result<Dictionary<string, decimal>>> Handle(GetMonthlyRevenueByCurrencyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var revenueByCurrency = await _policyAnalyticsService.GetMonthlyRevenueByCurrencyAsync(request.Month);

                _logger.LogDebug("Retrieved monthly revenue by currency for tenant {TenantId}", revenueByCurrency);

                return Result<Dictionary<string, decimal>>.Success(revenueByCurrency, "Aylık gelir para birimine göre getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly revenue by currency");
                return Result<Dictionary<string, decimal>>.InternalError("Aylık gelir para birimine göre alınırken beklenmeyen bir hata oluştu");
            }
        }
    }
}
