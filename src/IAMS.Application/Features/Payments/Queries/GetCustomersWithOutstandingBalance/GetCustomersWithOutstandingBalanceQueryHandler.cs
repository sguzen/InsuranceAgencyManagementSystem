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
                // OPTIMIZED: Calculate outstanding balance per customer using CustomerPayments
                // Outstanding Balance = Total Policy Premiums - Total Customer Payments
                var result = await _unitOfWork.Customers.AsQueryable()
                    .Where(c => !c.IsDeleted)
                    .Select(c => new
                    {
                        CustomerId = c.Id,
                        CustomerName = c.FirstName + " " + c.LastName,
                        CustomerEmail = c.Email ?? string.Empty,
                        // Total debt: sum of all active policy premiums (in TRY - base currency)
                        TotalDebt = c.Policies
                            .Where(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active)
                            .Sum(p => (decimal?)p.PremiumAmount) ?? 0,
                        // Total paid: sum of all completed customer payments (in TRY - base currency)
                        TotalPaid = c.CustomerPayments
                            .Where(cp => !cp.IsDeleted && cp.Status == Domain.Enums.PaymentStatus.Completed)
                            .Sum(cp => (decimal?)cp.Amount) ?? 0,
                        // Active policies count
                        ActivePoliciesCount = c.Policies
                            .Count(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active),
                        // Pending payments count
                        PendingPaymentsCount = c.CustomerPayments
                            .Count(cp => !cp.IsDeleted && cp.Status == Domain.Enums.PaymentStatus.Pending)
                    })
                    .Select(x => new
                    {
                        x.CustomerId,
                        x.CustomerName,
                        x.CustomerEmail,
                        OutstandingBalance = x.TotalDebt - x.TotalPaid,
                        x.ActivePoliciesCount,
                        x.PendingPaymentsCount
                    })
                    .Where(x => x.OutstandingBalance > 0)  // Only customers with outstanding balance (debt)
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
