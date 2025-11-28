using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenueByCurrency
{
    public class GetMonthlyRevenueByCurrencyQueryHandler : IRequestHandler<GetMonthlyRevenueByCurrencyQuery, Result<Dictionary<string, decimal>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetMonthlyRevenueByCurrencyQueryHandler> _logger;

        public GetMonthlyRevenueByCurrencyQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetMonthlyRevenueByCurrencyQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Dictionary<string, decimal>>> Handle(GetMonthlyRevenueByCurrencyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var revenueByCurrency = await _unitOfWork.Policies.GetMonthlyRevenueByCurrencyAsync(request.Month);

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
