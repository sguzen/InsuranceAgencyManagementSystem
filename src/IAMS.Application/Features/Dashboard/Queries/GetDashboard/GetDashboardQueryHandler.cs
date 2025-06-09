using IAMS.Application.Interfaces;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using IAMS.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

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
                var customers = await _unitOfWork.Customers.GetAllAsync();
                var policies = await _unitOfWork.Policies.GetAllAsync();
                var companies = await _unitOfWork.InsuranceCompanies.GetAllAsync();

                var dashboard = new DashboardDto
                {
                    TotalCustomers = customers.Count(),
                    ActiveCustomers = customers.Count(c => c.IsActive),
                    TotalPolicies = policies.Count(),
                    ActivePolicies = policies.Count(p => p.Status == PolicyStatus.Active),
                    ExpiringPoliciesThisMonth = policies.Count(p =>
                        p.EndDate.Year == DateTime.Now.Year &&
                        p.EndDate.Month == DateTime.Now.Month),
                    TotalPremiumAmount = policies.Sum(p => p.PremiumAmount),
                    TotalCommissionAmount = policies.Sum(p => p.CommissionAmount),
                    // Add more calculations as needed
                };

                return Result<DashboardDto>.Success(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard data");
                return Result<DashboardDto>.Failure("An error occurred while retrieving dashboard data");
            }
        }
    }
}