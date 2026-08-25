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
                _logger.LogInformation("Getting customers with balances per currency (customerId: {CustomerId})", request.CustomerId);

                // Premium totals aggregated in the database, per customer per currency.
                var premiumTotals = await _unitOfWork.Policies.AsQueryable()
                    .Where(p => !p.IsDeleted)
                    .Where(p => request.CustomerId == null || p.CustomerId == request.CustomerId)
                    .GroupBy(p => new { p.CustomerId, CurrencyCode = p.Currency.Code })
                    .Select(g => new
                    {
                        g.Key.CustomerId,
                        g.Key.CurrencyCode,
                        TotalPremium = g.Sum(p => p.PremiumAmount),
                        ActivePolicyCount = g.Sum(p => p.Status == PolicyStatus.Active ? 1 : 0)
                    })
                    .ToListAsync(cancellationToken);

                // Paid totals aggregated in the database, grouped by the policy's currency
                // (payments are recorded in the policy currency).
                var paidTotals = await _unitOfWork.PolicyPayments.AsQueryable()
                    .Where(pp => !pp.IsDeleted && !pp.Policy.IsDeleted)
                    .Where(pp => request.CustomerId == null || pp.Policy.CustomerId == request.CustomerId)
                    .GroupBy(pp => new { pp.Policy.CustomerId, CurrencyCode = pp.Policy.Currency.Code })
                    .Select(g => new
                    {
                        g.Key.CustomerId,
                        g.Key.CurrencyCode,
                        TotalPaid = g.Sum(pp => pp.Amount)
                    })
                    .ToListAsync(cancellationToken);

                var paidLookup = paidTotals.ToDictionary(x => (x.CustomerId, x.CurrencyCode), x => x.TotalPaid);

                // Contact/identity fields for the customers that have policies.
                var customers = await _unitOfWork.Customers.AsQueryable()
                    .Where(c => request.CustomerId == null || c.Id == request.CustomerId)
                    .Where(c => c.Policies.Any(p => !p.IsDeleted))
                    .Select(c => new
                    {
                        c.Id,
                        c.CustomerCode,
                        c.FirstName,
                        c.LastName,
                        c.Email,
                        c.MobilePhoneNumber,
                        c.Status,
                        c.CreatedOn
                    })
                    .ToListAsync(cancellationToken);

                var customerLookup = customers.ToDictionary(c => c.Id);

                // Every payment belongs to a policy, so premiumTotals covers all
                // customer/currency combinations with any activity.
                var result = new List<CustomerWithBalanceDto>();
                foreach (var group in premiumTotals)
                {
                    var totalPaid = paidLookup.TryGetValue((group.CustomerId, group.CurrencyCode), out var paid) ? paid : 0m;
                    var balance = group.TotalPremium - totalPaid;

                    // Include rows with an outstanding balance AND fully-paid rows, so the
                    // UI can show premium totals (#517). Consumers that only care about
                    // debt filter on Balance != 0 themselves.
                    if (balance == 0 && group.TotalPremium == 0)
                    {
                        continue;
                    }

                    if (!customerLookup.TryGetValue(group.CustomerId, out var customer))
                    {
                        continue;
                    }

                    result.Add(new CustomerWithBalanceDto
                    {
                        Id = group.CustomerId,
                        CustomerCode = customer.CustomerCode,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        MobilePhoneNumber = customer.MobilePhoneNumber,
                        Status = customer.Status,
                        CreatedOn = customer.CreatedOn,
                        ActivePolicyCount = group.ActivePolicyCount,
                        Currency = group.CurrencyCode,
                        Balance = balance,
                        TotalPremium = group.TotalPremium,
                        TotalPaid = totalPaid
                    });
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
