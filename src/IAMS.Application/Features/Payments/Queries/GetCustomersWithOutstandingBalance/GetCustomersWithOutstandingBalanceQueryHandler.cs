using IAMS.Application.DTOs.Payment;
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
                var outstandingBalances = await _unitOfWork.PolicyPayments.GetOutstandingBalanceByCustomerAsync();

                var result = new List<CustomerOutstandingBalanceDto>();

                foreach (var balance in outstandingBalances.Where(b => b.Value > 0))
                {
                    var customer = await _unitOfWork.Customers.GetByIdAsync(balance.Key);
                    if (customer != null)
                    {
                        var customerPolicies = await _unitOfWork.Policies.GetPoliciesByCustomerIdAsync(customer.Id);
                        var activePoliciesCount = customerPolicies.Count();

                        var pendingPayments = await _unitOfWork.PolicyPayments.GetPaymentsByDateRangeAsync(
                            DateTime.MinValue, DateTime.MaxValue);
                        var pendingPaymentsCount = pendingPayments
                            .Count(p => p.Policy.CustomerId == customer.Id &&
                                       p.Status == Domain.Enums.PaymentStatus.Pending);

                        result.Add(new CustomerOutstandingBalanceDto
                        {
                            CustomerId = customer.Id,
                            CustomerName = $"{customer.FirstName} {customer.LastName}",
                            CustomerEmail = customer.Email,
                            OutstandingBalance = balance.Value,
                            ActivePoliciesCount = activePoliciesCount,
                            PendingPaymentsCount = pendingPaymentsCount
                        });
                    }
                }

                return result.OrderByDescending(x => x.OutstandingBalance).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with outstanding balance");
                throw;
            }
        }
    }
}
