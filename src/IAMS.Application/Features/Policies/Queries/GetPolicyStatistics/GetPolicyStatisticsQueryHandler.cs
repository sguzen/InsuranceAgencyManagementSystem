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
                // Get policy count by status
                var policyCountByStatus = await _unitOfWork.Policies.GetPolicyCountByStatusAsync();

                // Get all policies for additional calculations
                var allPolicies = await _unitOfWork.Policies.GetAllAsync();
                var activePolicies = allPolicies.Where(p => p.Status == IAMS.Domain.Enums.PolicyStatus.Active && !p.IsDeleted).ToList();

                // Calculate financial metrics
                var totalPremiums = allPolicies.Where(p => !p.IsDeleted).Sum(p => p.PremiumAmount);
                var totalCommissions = allPolicies.Where(p => !p.IsDeleted).Sum(p => p.CommissionAmount);
                var averagePremium = allPolicies.Any(p => !p.IsDeleted) ? allPolicies.Where(p => !p.IsDeleted).Average(p => p.PremiumAmount) : 0;
                var averageCommissionRate = allPolicies.Any(p => !p.IsDeleted) ? allPolicies.Where(p => !p.IsDeleted).Average(p => p.CommissionRate) : 0;

                // Calculate period statistics
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var newPoliciesThisMonth = allPolicies.Count(p => p.CreatedOn >= startOfMonth && !p.IsDeleted);
                // TODO: Add ParentPolicyId to Policy entity to properly track renewals
                var renewalsThisMonth = 0; // Will be: allPolicies.Count(p => p.CreatedOn >= startOfMonth && p.ParentPolicyId.HasValue && !p.IsDeleted);
                var cancellationsThisMonth = allPolicies.Count(p => p.Status == IAMS.Domain.Enums.PolicyStatus.Cancelled && p.ModifiedOn >= startOfMonth && !p.IsDeleted);
                var expirationsThisMonth = allPolicies.Count(p => p.EndDate >= startOfMonth && p.EndDate < startOfMonth.AddMonths(1) && p.EndDate <= now && !p.IsDeleted);

                // Calculate performance metrics
                var totalActivePoliciesLastMonth = allPolicies.Count(p => p.CreatedOn < startOfMonth && p.Status == IAMS.Domain.Enums.PolicyStatus.Active && !p.IsDeleted);
                var renewalRate = totalActivePoliciesLastMonth > 0 ? (double)renewalsThisMonth / totalActivePoliciesLastMonth * 100 : 0;
                var cancellationRate = totalActivePoliciesLastMonth > 0 ? (double)cancellationsThisMonth / totalActivePoliciesLastMonth * 100 : 0;
                var lastMonth = allPolicies.Count(p => p.CreatedOn >= startOfMonth.AddMonths(-1) && p.CreatedOn < startOfMonth && !p.IsDeleted);
                var growthRate = lastMonth > 0 ? ((double)(newPoliciesThisMonth - lastMonth) / lastMonth) * 100 : 0;

                // Get policies by type
                var policiesByType = allPolicies
                    .Where(p => !p.IsDeleted && p.PolicyType != null)
                    .GroupBy(p => p.PolicyType.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueByType = allPolicies
                    .Where(p => !p.IsDeleted && p.PolicyType != null)
                    .GroupBy(p => p.PolicyType.Name)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.PremiumAmount));

                // Get policies by company
                var policiesByCompany = allPolicies
                    .Where(p => !p.IsDeleted && p.InsuranceCompany != null)
                    .GroupBy(p => p.InsuranceCompany.Name)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueByCompany = allPolicies
                    .Where(p => !p.IsDeleted && p.InsuranceCompany != null)
                    .GroupBy(p => p.InsuranceCompany.Name)
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.PremiumAmount));

                // Get policies by month (last 12 months)
                var monthlyData = await _unitOfWork.Policies.GetRevenueByMonthAsync(12);

                var statistics = new PolicyStatisticsDto
                {
                    // Count statistics
                    TotalPolicies = await _unitOfWork.Policies.GetPolicyCountAsync(),
                    ActivePolicies = await _unitOfWork.Policies.GetActivePolicyCountAsync(),
                    DraftPolicies = policyCountByStatus.GetValueOrDefault(IAMS.Domain.Enums.PolicyStatus.Draft, 0),
                    ExpiredPolicies = await _unitOfWork.Policies.GetExpiredPolicyCountAsync(),
                    CancelledPolicies = policyCountByStatus.GetValueOrDefault(IAMS.Domain.Enums.PolicyStatus.Cancelled, 0),
                    SuspendedPolicies = policyCountByStatus.GetValueOrDefault(IAMS.Domain.Enums.PolicyStatus.Suspended, 0),
                    ExpiringPolicies = await _unitOfWork.Policies.GetExpiringPolicyCountAsync(30),

                    // Financial statistics
                    TotalPremiums = totalPremiums,
                    TotalCommissions = totalCommissions,
                    MonthlyRevenue = await _unitOfWork.Policies.GetMonthlyRevenueAsync(),
                    YearlyRevenue = await _unitOfWork.Policies.GetYearlyRevenueAsync(),
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
                    RevenueByMonth = monthlyData
                };

                // Calculate policies by month from the monthly data
                var policiesByMonth = new Dictionary<string, int>();
                foreach (var kvp in monthlyData)
                {
                    var monthPolicies = allPolicies.Count(p =>
                        !p.IsDeleted &&
                        p.CreatedOn.ToString("yyyy-MM") == kvp.Key);
                    policiesByMonth[kvp.Key] = monthPolicies;
                }
                statistics.PoliciesByMonth = policiesByMonth;

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
