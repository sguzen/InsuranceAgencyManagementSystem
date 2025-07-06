using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Policies.Queries.GetMonthlyRevenue
{
    public class GetMonthlyRevenueQueryHandler : IRequestHandler<GetMonthlyRevenueQuery, Result<decimal>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetMonthlyRevenueQueryHandler> _logger;

        public GetMonthlyRevenueQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<GetMonthlyRevenueQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(GetMonthlyRevenueQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result<decimal>.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var tenantId = _currentTenantService.TenantId.Value;
                var revenue = await _unitOfWork.Policies.GetMonthlyRevenueAsync(tenantId);

                _logger.LogDebug("Retrieved monthly revenue {Revenue} for tenant {TenantId}", revenue, tenantId);

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