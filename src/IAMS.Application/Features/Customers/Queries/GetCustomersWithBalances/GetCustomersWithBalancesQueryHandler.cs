using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IAMS.Shared.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.DTOs.Customer;
using IAMS.Domain.Enums;

namespace IAMS.Application.Features.Customers.Queries.GetCustomersWithBalances
{
    public class GetCustomersWithBalancesQueryHandler : IRequestHandler<GetCustomersWithBalancesQuery, Result<List<CustomerWithBalanceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCustomersWithBalancesQueryHandler> _logger;

        public GetCustomersWithBalancesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCustomersWithBalancesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<List<CustomerWithBalanceDto>>> Handle(GetCustomersWithBalancesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting customers with balances per currency");

                // Get customers with their policies and payments, grouped by customer and currency
                var customersWithBalances = await (
                    from customer in _unitOfWork.Customers.AsQueryable().Where(c => !c.IsDeleted)
                    from policy in _unitOfWork.Policies.AsQueryable()
                        .Where(p => p.CustomerId == customer.Id && !p.IsDeleted)
                        .DefaultIfEmpty()
                    group new { customer, policy } by new
                    {
                        CustomerId = customer.Id,
                        customer.CustomerCode,
                        customer.FirstName,
                        customer.LastName,
                        customer.Email,
                        customer.MobilePhoneNumber,
                        customer.Status,
                        customer.CreatedOn,
                        Currency = policy != null ? policy.Currency.Code : "TRY"
                    } into customerCurrencyGroup
                    let activePolicies = customerCurrencyGroup.Where(x => x.policy != null && x.policy.Status == PolicyStatus.Active)
                    let totalPremium = customerCurrencyGroup.Where(x => x.policy != null).Sum(x => x.policy.PremiumAmount)
                    select new
                    {
                        customerCurrencyGroup.Key.CustomerId,
                        customerCurrencyGroup.Key.CustomerCode,
                        customerCurrencyGroup.Key.FirstName,
                        customerCurrencyGroup.Key.LastName,
                        customerCurrencyGroup.Key.Email,
                        customerCurrencyGroup.Key.MobilePhoneNumber,
                        customerCurrencyGroup.Key.Status,
                        customerCurrencyGroup.Key.CreatedOn,
                        customerCurrencyGroup.Key.Currency,
                        TotalPremium = totalPremium,
                        ActivePolicyCount = activePolicies.Count(),
                        PolicyIds = customerCurrencyGroup.Where(x => x.policy != null).Select(x => x.policy.Id).ToList()
                    }
                ).ToListAsync(cancellationToken);

                // Calculate total paid for each customer-currency combination
                var result = new List<CustomerWithBalanceDto>();

                foreach (var item in customersWithBalances)
                {
                    decimal totalPaid = 0;

                    if (item.PolicyIds.Any())
                    {
                        totalPaid = await _unitOfWork.PolicyPayments.AsQueryable()
                            .Where(p => item.PolicyIds.Contains(p.PolicyId) && !p.IsDeleted)
                            .SumAsync(p => p.Amount, cancellationToken);
                    }

                    var balance = item.TotalPremium - totalPaid;

                    // Only include if there's a balance or active policies
                    if (balance != 0 || item.ActivePolicyCount > 0)
                    {
                        result.Add(new CustomerWithBalanceDto
                        {
                            Id = item.CustomerId,
                            CustomerCode = item.CustomerCode,
                            FirstName = item.FirstName,
                            LastName = item.LastName,
                            Email = item.Email,
                            MobilePhoneNumber = item.MobilePhoneNumber,
                            Status = item.Status,
                            CreatedOn = item.CreatedOn,
                            ActivePolicyCount = item.ActivePolicyCount,
                            Currency = item.Currency,
                            Balance = balance,
                            TotalPremium = item.TotalPremium,
                            TotalPaid = totalPaid
                        });
                    }
                }

                _logger.LogInformation("Found {Count} customer-currency combinations with balances", result.Count);
                return Result<List<CustomerWithBalanceDto>>.Success(result.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.Currency).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customers with balances");
                return Result<List<CustomerWithBalanceDto>>.Failure("Müşteri bakiyeleri getirilirken hata oluştu", ex.Message);
            }
        }
    }
}
