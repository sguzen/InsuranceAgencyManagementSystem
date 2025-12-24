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
                // For each customer, calculate total debt (policy premiums) minus total paid (policy payments)
                // and filter to only those with positive balance
                var baseQuery = _unitOfWork.Customers.AsQueryable()
                    .Where(c => !c.IsDeleted) // Only active customers
                    .Select(c => new
                    {
                        Customer = c,
                        TotalDebt = c.Policies
                            .Where(p => !p.IsDeleted)
                            .Sum(p => (decimal?)p.PremiumAmount) ?? 0,
                        TotalPaid = c.Policies
                            .Where(p => !p.IsDeleted)
                            .SelectMany(p => p.PolicyPayments)
                            .Where(pp => pp.Status == Domain.Enums.PaymentStatus.Completed && !pp.IsDeleted)
                            .Sum(pp => (decimal?)pp.Amount) ?? 0,
                        ActivePoliciesCount = c.Policies.Count(p => p.Status == Domain.Enums.PolicyStatus.Active && !p.IsDeleted),
                        PendingPaymentsCount = c.Policies
                            .Where(p => !p.IsDeleted)
                            .SelectMany(p => p.PolicyPayments)
                            .Count(pp => pp.Status == Domain.Enums.PaymentStatus.Pending && !pp.IsDeleted)
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
                    var result = await baseQuery
                        .Take(request.Limit.Value)
                        .ToListAsync(cancellationToken);
                    return result;
                }
                else
                {
                    var result = await baseQuery.ToListAsync(cancellationToken);
                    return result;
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
