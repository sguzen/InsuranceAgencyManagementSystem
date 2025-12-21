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
                // Calculate outstanding balance per customer based on policy premiums vs policy payments
                // Outstanding Balance = Total Policy Premiums - Total Policy Payments
                var result = await (
                    from c in _unitOfWork.Customers.AsQueryable().Where(c => !c.IsDeleted)
                    let totalDebt = c.Policies
                        .Where(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active)
                        .Sum(p => (decimal?)p.PremiumAmount) ?? 0
                    let totalPaid = c.Policies
                        .Where(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active)
                        .SelectMany(p => p.PolicyPayments)
                        .Where(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Completed)
                        .Sum(pp => (decimal?)pp.Amount) ?? 0
                    let activePoliciesCount = c.Policies
                        .Count(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active)
                    let pendingPaymentsCount = c.Policies
                        .Where(p => !p.IsDeleted && p.Status == Domain.Enums.PolicyStatus.Active)
                        .SelectMany(p => p.PolicyPayments)
                        .Count(pp => !pp.IsDeleted && pp.Status == Domain.Enums.PaymentStatus.Pending)
                    let outstandingBalance = totalDebt - totalPaid
                    where outstandingBalance > 0
                    orderby outstandingBalance descending
                    select new CustomerOutstandingBalanceDto
                    {
                        CustomerId = c.Id,
                        CustomerName = c.FirstName + " " + c.LastName,
                        CustomerEmail = c.Email ?? string.Empty,
                        OutstandingBalance = outstandingBalance,
                        ActivePoliciesCount = activePoliciesCount,
                        PendingPaymentsCount = pendingPaymentsCount
                    }
                ).ToListAsync(cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with outstanding balance");
                throw;
            }
        }
    }
}
