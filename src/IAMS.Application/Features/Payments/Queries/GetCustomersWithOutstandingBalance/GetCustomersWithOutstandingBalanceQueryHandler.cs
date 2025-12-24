using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace IAMS.Application.Features.Payments.Queries.GetCustomersWithOutstandingBalance
{
    public class GetCustomersWithOutstandingBalanceQueryHandler : IRequestHandler<GetCustomersWithOutstandingBalanceQuery, List<CustomerOutstandingBalanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCustomersWithOutstandingBalanceQueryHandler> _logger;

        public GetCustomersWithOutstandingBalanceQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCustomersWithOutstandingBalanceQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<CustomerOutstandingBalanceDto>> Handle(GetCustomersWithOutstandingBalanceQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // OPTIMIZED: Use AsNoTracking for read-only query and simplify aggregations
                // This approach loads customers with their policies and policy payments in one go,
                // then performs calculations in memory for better performance
                var customersWithPolicies = await _unitOfWork.Customers.AsQueryable()
                    .AsNoTracking()
                    .Where(c => !c.IsDeleted)
                    .Include(c => c.Policies.Where(p => !p.IsDeleted))
                        .ThenInclude(p => p.PolicyPayments.Where(pp => !pp.IsDeleted))
                    .ToListAsync(cancellationToken);

                // Calculate balances in memory (more efficient than complex LINQ to SQL)
                var customersWithBalance = customersWithPolicies
                    .Select(c => new
                    {
                        Customer = c,
                        TotalDebt = c.Policies.Sum(p => p.PremiumAmount),
                        TotalPaid = c.Policies
                            .SelectMany(p => p.PolicyPayments)
                            .Where(pp => pp.Status == Domain.Enums.PaymentStatus.Completed)
                            .Sum(pp => pp.Amount),
                        ActivePoliciesCount = c.Policies.Count(p => p.Status == Domain.Enums.PolicyStatus.Active),
                        PendingPaymentsCount = c.Policies
                            .SelectMany(p => p.PolicyPayments)
                            .Count(pp => pp.Status == Domain.Enums.PaymentStatus.Pending)
                    })
                    .Where(x => x.TotalDebt > x.TotalPaid) // Only customers with outstanding debt
                    .Select(x => new CustomerOutstandingBalanceDto
                    {
                        CustomerId = x.Customer.Id,
                        CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                        CustomerEmail = x.Customer.Email ?? string.Empty,
                        OutstandingBalance = x.TotalDebt - x.TotalPaid,
                        ActivePoliciesCount = x.ActivePoliciesCount,
                        PendingPaymentsCount = x.PendingPaymentsCount
                    })
                    .OrderByDescending(x => x.OutstandingBalance);

                // Apply limit if specified
                if (request.Limit.HasValue && request.Limit.Value > 0)
                {
                    return customersWithBalance.Take(request.Limit.Value).ToList();
                }
                else
                {
                    return customersWithBalance.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with outstanding balance");
                throw;
            }
        }
    }
}
