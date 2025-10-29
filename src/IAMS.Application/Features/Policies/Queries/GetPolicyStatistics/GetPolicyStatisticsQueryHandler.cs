using IAMS.Application.DTOs.Policy;
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
        private readonly ILogger<GetPolicyStatisticsQueryHandler> _logger;

        public GetPolicyStatisticsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetPolicyStatisticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<PolicyStatisticsDto>> Handle(GetPolicyStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {

                var statistics = new PolicyStatisticsDto
                {
                    TotalPolicies = await _unitOfWork.Policies.GetPolicyCountAsync(),
                    ActivePolicies = await _unitOfWork.Policies.GetActivePolicyCountAsync(),
                    ExpiringPolicies = await _unitOfWork.Policies.GetExpiringPolicyCountAsync(30),
                    ExpiredPolicies = await _unitOfWork.Policies.GetExpiredPolicyCountAsync(),
                    MonthlyRevenue = await _unitOfWork.Policies.GetMonthlyRevenueAsync(),
                    YearlyRevenue = await _unitOfWork.Policies.GetYearlyRevenueAsync()
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
