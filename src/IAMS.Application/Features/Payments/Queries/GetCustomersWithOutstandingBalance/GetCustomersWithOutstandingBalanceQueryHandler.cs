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
                // Simple, straightforward query: For each customer with active policies,
                // calculate total debt (policy premiums) minus total paid (policy payments)
                var baseQuery = _unitOfWork.Customers.AsQueryable()
                    .Select(c => new CustomerOutstandingBalanceDto
                    {
                        CustomerId = c.Id,
                        CustomerName = c.FirstName + " " + c.LastName,
                        CustomerEmail = c.Email ?? string.Empty,
                        // Total debt from active policies
                        OutstandingBalance = c.Policies
                            .Where(p => p.Status == Domain.Enums.PolicyStatus.Active)
                            .Sum(p => (decimal?)p.PremiumAmount) ?? 0
                            -
                            c.Policies
                            .Where(p => p.Status == Domain.Enums.PolicyStatus.Active)
                            .SelectMany(p => p.PolicyPayments)
                            .Where(pp => pp.Status == Domain.Enums.PaymentStatus.Completed)
                            .Sum(pp => (decimal?)pp.Amount) ?? 0,
                        // Count of active policies
                        ActivePoliciesCount = c.Policies
                            .Count(p => p.Status == Domain.Enums.PolicyStatus.Active),
                        // Count of pending payments
                        PendingPaymentsCount = c.Policies
                            .Where(p => p.Status == Domain.Enums.PolicyStatus.Active)
                            .SelectMany(p => p.PolicyPayments)
                            .Count(pp => pp.Status == Domain.Enums.PaymentStatus.Pending)
                    })
                    .Where(x => x.OutstandingBalance > 0) // Only customers with outstanding debt
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
