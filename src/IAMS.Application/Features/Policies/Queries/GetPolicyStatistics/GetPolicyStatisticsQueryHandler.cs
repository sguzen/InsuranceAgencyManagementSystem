using IAMS.Application.Interfaces;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyStatistics
{
    public class GetPolicyStatisticsQueryHandler : IRequestHandler<GetPolicyStatisticsQuery, Result<PolicyStatisticsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTenantService _currentTenantService;
        private readonly ILogger<GetPolicyStatisticsQueryHandler> _logger;

        public GetPolicyStatisticsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentTenantService currentTenantService,
            ILogger<GetPolicyStatisticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentTenantService = currentTenantService;
            _logger = logger;
        }

        public async Task<Result<PolicyStatisticsDto>> Handle(GetPolicyStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (!_currentTenantService.HasTenant || _currentTenantService.TenantId == null)
                {
                    return Result<PolicyStatisticsDto>.Unauthorized("Kiracı bağlamı bulunamadı");
                }

                var tenantId = _currentTenantService.TenantId.Value;

                var statistics = new PolicyStatisticsDto
                {
                    TotalPolicies = await _unitOfWork.Policies.GetPolicyCountAsync(tenantId),
                    ActivePolicies = await _unitOfWork.Policies.GetActivePolicyCountAsync(tenantId),
                    ExpiringPolicies = await _unitOfWork.Policies.GetExpiringPolicyCountAsync(30, tenantId),
                    ExpiredPolicies = await _unitOfWork.Policies.GetExpiredPolicyCountAsync(tenantId),
                    MonthlyRevenue = await _unitOfWork.Policies.GetMonthlyRevenueAsync(tenantId),
                    YearlyRevenue = await _unitOfWork.Policies.GetYearlyRevenueAsync(tenantId)
                };

                return Result<PolicyStatisticsDto>.Success(statistics, "Poliçe istatistikleri başarıyla getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy statistics");
                return Result<PolicyStatisticsDto>.InternalError("Poliçe istatistikleri getirilirken beklenmeyen bir hata oluştu");
            }
        }
    }
}
