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
                // OPTIMIZED: Single database query with aggregations
                // No N+1 queries, no loading all payments multiple times
                // Everything calculated in the database
                var result = await _unitOfWork.Customers.AsQueryable()
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        CustomerId = c.Id,
                        CustomerName = c.FirstName + " " + c.LastName,
                        CustomerEmail = c.Email ?? string.Empty,
                        // Outstanding balance: sum of (premium - total paid) for all policies
                        OutstandingBalance = c.Policies
                            .Where(p => !p.IsDeleted)
                            .Sum(p => p.PremiumAmount -
                                p.PolicyPayments
                                    .Where(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Completed)
                                    .Sum(pp => (decimal?)pp.Amount) ?? 0),
                        // Active policies count
                        ActivePoliciesCount = c.Policies
                            .Count(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active),
                        // Pending payments count across all customer's policies
                        PendingPaymentsCount = c.Policies
                            .Where(p => !p.IsDeleted)
                            .SelectMany(p => p.PolicyPayments)
                            .Count(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Pending)
                    })
                    .Where(x => x.OutstandingBalance > 0)  // Only customers with outstanding balance
                    .OrderByDescending(x => x.OutstandingBalance)
                    .ToListAsync(cancellationToken);

                // Map to DTO
                return result.Select(x => new CustomerOutstandingBalanceDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    CustomerEmail = x.CustomerEmail,
                    OutstandingBalance = x.OutstandingBalance,
                    ActivePoliciesCount = x.ActivePoliciesCount,
                    PendingPaymentsCount = x.PendingPaymentsCount
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with outstanding balance");
                throw;
            }
        }
    }
}
