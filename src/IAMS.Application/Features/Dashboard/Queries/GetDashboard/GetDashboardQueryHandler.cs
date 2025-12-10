using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IAMS.Application.Models;

namespace IAMS.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Result<DashboardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetDashboardQueryHandler> _logger;

        public GetDashboardQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDashboardQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<DashboardDto>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // OPTIMIZED: Use database aggregations instead of loading everything into memory
                // Each query runs independently and efficiently in SQL

                var customersQuery = _unitOfWork.Customers.AsQueryable().Where(c => !c.IsDeleted);
                var policiesQuery = _unitOfWork.Policies.AsQueryable().Where(p => !p.IsDeleted);

                // Run all queries in parallel for better performance
                var totalCustomersTask = customersQuery.CountAsync(cancellationToken);
                var activeCustomersTask = customersQuery.CountAsync(c => c.IsActive, cancellationToken);
                var totalPoliciesTask = policiesQuery.CountAsync(cancellationToken);
                var activePoliciesTask = policiesQuery.CountAsync(p => p.Status == PolicyStatus.Active, cancellationToken);

                var now = DateTime.Now;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                var expiringPoliciesTask = policiesQuery.CountAsync(p =>
                    p.EndDate >= startOfMonth && p.EndDate <= endOfMonth, cancellationToken);

                var totalPremiumTask = policiesQuery.SumAsync(p => p.PremiumAmount, cancellationToken);
                var totalCommissionTask = policiesQuery.SumAsync(p => p.CommissionAmount, cancellationToken);

                // Wait for all tasks to complete
                await Task.WhenAll(
                    totalCustomersTask,
                    activeCustomersTask,
                    totalPoliciesTask,
                    activePoliciesTask,
                    expiringPoliciesTask,
                    totalPremiumTask,
                    totalCommissionTask);

                var dashboard = new DashboardDto
                {
                    TotalCustomers = await totalCustomersTask,
                    ActiveCustomers = await activeCustomersTask,
                    TotalPolicies = await totalPoliciesTask,
                    ActivePolicies = await activePoliciesTask,
                    ExpiringPoliciesThisMonth = await expiringPoliciesTask,
                    TotalPremiumAmount = await totalPremiumTask,
                    TotalCommissionAmount = await totalCommissionTask,
                };

                return Result<DashboardDto>.Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard data");
                return Result<DashboardDto>.InternalError("An error occurred while retrieving dashboard data");
            }
        }
    }
}