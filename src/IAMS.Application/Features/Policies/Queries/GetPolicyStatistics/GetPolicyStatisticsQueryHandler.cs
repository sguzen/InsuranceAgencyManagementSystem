using IAMS.Shared.QueryParams;
using IAMS.Application.Interfaces;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Application.Interfaces.Services;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IAMS.Application.Models;
using IAMS.Application.DTOs.Policy;

namespace IAMS.Application.Features.Policies.Queries.GetPolicyStatistics
{
    public class GetPolicyStatisticsQueryHandler : IRequestHandler<GetPolicyStatisticsQuery, Result<PolicyStatisticsDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPolicyAnalyticsService _policyAnalyticsService;
        private readonly ILogger<GetPolicyStatisticsQueryHandler> _logger;

        public GetPolicyStatisticsQueryHandler(
            IUnitOfWork unitOfWork,
            IPolicyAnalyticsService policyAnalyticsService,
            ILogger<GetPolicyStatisticsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _policyAnalyticsService = policyAnalyticsService;
            _logger = logger;
        }

        public async Task<Result<PolicyStatisticsDto>> Handle(GetPolicyStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // OPTIMIZED: Use database aggregations instead of loading all policies into memory
                var policiesQuery = _unitOfWork.Policies.AsQueryable().Where(p => !p.IsDeleted);

                // Get policy count by status
                var policyCountByStatus = await _policyAnalyticsService.GetPolicyCountByStatusAsync();

                // Calculate period timestamps
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                // Calculate financial metrics with parallel database aggregations
                var totalPremiums = await policiesQuery.SumAsync(p => p.PremiumAmount, cancellationToken);
                var totalCommissions = await policiesQuery.SumAsync(p => p.CommissionAmount, cancellationToken);
                var policyCount = await policiesQuery.CountAsync(cancellationToken);
                var averagePremium = policyCount > 0 ? await policiesQuery.AverageAsync(p => p.PremiumAmount, cancellationToken) : 0;
                var averageCommissionRate = policyCount > 0 ? await policiesQuery.AverageAsync(p => p.CommissionRate, cancellationToken) : 0;

                // Calculate period statistics with database queries
                var newPoliciesThisMonth = await policiesQuery
                    .CountAsync(p => p.CreatedOn >= startOfMonth && !p.ParentPolicyId.HasValue, cancellationToken);

                var renewalsThisMonth = await policiesQuery
                    .CountAsync(p => p.CreatedOn >= startOfMonth && p.ParentPolicyId.HasValue, cancellationToken);

                var cancellationsThisMonth = await policiesQuery
                    .CountAsync(p => p.Status == IAMS.Domain.Enums.PolicyStatus.Cancelled && p.ModifiedOn >= startOfMonth, cancellationToken);

                var expirationsThisMonth = await policiesQuery
                    .CountAsync(p => p.EndDate >= startOfMonth && p.EndDate < endOfMonth && p.EndDate <= now, cancellationToken);

                // Calculate performance metrics
                var totalActivePoliciesLastMonth = await policiesQuery
                    .CountAsync(p => p.CreatedOn < startOfMonth && p.Status == IAMS.Domain.Enums.PolicyStatus.Active, cancellationToken);

                var renewalRate = totalActivePoliciesLastMonth > 0 ? (double)renewalsThisMonth / totalActivePoliciesLastMonth * 100 : 0;
                var cancellationRate = totalActivePoliciesLastMonth > 0 ? (double)cancellationsThisMonth / totalActivePoliciesLastMonth * 100 : 0;

                var lastMonthCount = await policiesQuery
                    .CountAsync(p => p.CreatedOn >= startOfLastMonth && p.CreatedOn < startOfMonth, cancellationToken);

                var growthRate = lastMonthCount > 0 ? ((double)(newPoliciesThisMonth - lastMonthCount) / lastMonthCount) * 100 : 0;

                // Get policies by type with database grouping
                var policiesByType = await policiesQuery
                    .Where(p => p.PolicyType != null)
                    .GroupBy(p => p.PolicyType.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Name, x => x.Count, cancellationToken);

                var revenueByType = await policiesQuery
                    .Where(p => p.PolicyType != null)
                    .GroupBy(p => p.PolicyType.Name)
                    .Select(g => new { Name = g.Key, Revenue = g.Sum(p => p.PremiumAmount) })
                    .ToDictionaryAsync(x => x.Name, x => x.Revenue, cancellationToken);

                // Get policies by company with database grouping
                var policiesByCompany = await policiesQuery
                    .Where(p => p.InsuranceCompany != null)
                    .GroupBy(p => p.InsuranceCompany.Name)
                    .Select(g => new { Name = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Name, x => x.Count, cancellationToken);

                var revenueByCompany = await policiesQuery
                    .Where(p => p.InsuranceCompany != null)
                    .GroupBy(p => p.InsuranceCompany.Name)
                    .Select(g => new { Name = g.Key, Revenue = g.Sum(p => p.PremiumAmount) })
                    .ToDictionaryAsync(x => x.Name, x => x.Revenue, cancellationToken);

                // Get policies by month (last 12 months)
                var monthlyData = await _policyAnalyticsService.GetRevenueByMonthAsync(12);

                // Calculate policies by month with database query
                var policiesByMonth = new Dictionary<string, int>();
                foreach (var kvp in monthlyData)
                {
                    // Parse the month key (assuming format "yyyy-MM")
                    if (DateTime.TryParseExact(kvp.Key, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out var monthDate))
                    {
                        var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                        var monthEnd = monthStart.AddMonths(1);

                        var monthPoliciesCount = await policiesQuery
                            .CountAsync(p => p.CreatedOn >= monthStart && p.CreatedOn < monthEnd, cancellationToken);

                        policiesByMonth[kvp.Key] = monthPoliciesCount;
                    }
                }

                var statistics = new PolicyStatisticsDto
                {
                    // Count statistics
                    TotalPolicies = await _policyAnalyticsService.GetPolicyCountAsync(),
                    ActivePolicies = await _policyAnalyticsService.GetActivePolicyCountAsync(),
                    DraftPolicies = policyCountByStatus.GetValueOrDefault(IAMS.Domain.Enums.PolicyStatus.Draft, 0),
                    ExpiredPolicies = await _policyAnalyticsService.GetExpiredPolicyCountAsync(),
                    CancelledPolicies = policyCountByStatus.GetValueOrDefault(IAMS.Domain.Enums.PolicyStatus.Cancelled, 0),
                    SuspendedPolicies = policyCountByStatus.GetValueOrDefault(IAMS.Domain.Enums.PolicyStatus.Suspended, 0),
                    ExpiringPolicies = await _policyAnalyticsService.GetExpiringPolicyCountAsync(30),

                    // Financial statistics
                    TotalPremiums = totalPremiums,
                    TotalCommissions = totalCommissions,
                    MonthlyRevenue = await _policyAnalyticsService.GetMonthlyRevenueAsync(),
                    YearlyRevenue = await _policyAnalyticsService.GetYearlyRevenueAsync(),
                    AveragePremium = averagePremium,
                    AverageCommissionRate = averageCommissionRate,

                    // Period statistics
                    NewPoliciesThisMonth = newPoliciesThisMonth,
                    RenewalsThisMonth = renewalsThisMonth,
                    CancellationsThisMonth = cancellationsThisMonth,
                    ExpirationsThisMonth = expirationsThisMonth,

                    // Performance metrics
                    RenewalRate = renewalRate,
                    CancellationRate = cancellationRate,
                    GrowthRate = growthRate,

                    // By category breakdowns
                    PoliciesByType = policiesByType,
                    RevenueByType = revenueByType,
                    PoliciesByCompany = policiesByCompany,
                    RevenueByCompany = revenueByCompany,
                    RevenueByMonth = monthlyData,
                    PoliciesByMonth = policiesByMonth
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
