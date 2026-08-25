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

                // Get all policies with their related data
                var policies = await _unitOfWork.Policies.AsQueryable()
                    .Where(p => !p.IsDeleted)
                    .Include(p => p.Customer)
                    .Include(p => p.Currency)
                    .Select(p => new
                    {
                        p.Id,
                        p.CustomerId,
                        CustomerCode = p.Customer.CustomerCode,
                        FirstName = p.Customer.FirstName,
                        LastName = p.Customer.LastName,
                        Email = p.Customer.Email,
                        MobilePhoneNumber = p.Customer.MobilePhoneNumber,
                        Status = p.Customer.Status,
                        CreatedOn = p.Customer.CreatedOn,
                        CurrencyCode = p.Currency.Code,
                        p.PremiumAmount,
                        PolicyStatus = p.Status
                    })
                    .ToListAsync(cancellationToken);

                // Get all payments
                var payments = await _unitOfWork.PolicyPayments.AsQueryable()
                    .Where(p => !p.IsDeleted)
                    .Select(p => new
                    {
                        p.PolicyId,
                        p.Amount
                    })
                    .ToListAsync(cancellationToken);

                // Group by customer and currency
                var customerCurrencyGroups = policies
                    .GroupBy(p => new { p.CustomerId, p.CurrencyCode })
                    .Select(g => new
                    {
                        g.Key.CustomerId,
                        g.Key.CurrencyCode,
                        CustomerCode = g.First().CustomerCode,
                        FirstName = g.First().FirstName,
                        LastName = g.First().LastName,
                        Email = g.First().Email,
                        MobilePhoneNumber = g.First().MobilePhoneNumber,
                        Status = g.First().Status,
                        CreatedOn = g.First().CreatedOn,
                        TotalPremium = g.Sum(p => p.PremiumAmount),
                        ActivePolicyCount = g.Count(p => p.PolicyStatus == PolicyStatus.Active),
                        PolicyIds = g.Select(p => p.Id).ToList()
                    })
                    .ToList();

                // Calculate balances
                var result = new List<CustomerWithBalanceDto>();

                foreach (var group in customerCurrencyGroups)
                {
                    var totalPaid = payments
                        .Where(p => group.PolicyIds.Contains(p.PolicyId))
                        .Sum(p => p.Amount);

                    var balance = group.TotalPremium - totalPaid;

                    // Include every customer/currency with activity: rows with an outstanding
                    // balance AND fully-paid rows, so the UI can show premium totals (#517).
                    // Consumers that only care about debt filter on Balance != 0 themselves.
                    if (balance != 0 || group.TotalPremium != 0)
                    {
                        result.Add(new CustomerWithBalanceDto
                        {
                            Id = group.CustomerId,
                            CustomerCode = group.CustomerCode,
                            FirstName = group.FirstName,
                            LastName = group.LastName,
                            Email = group.Email,
                            MobilePhoneNumber = group.MobilePhoneNumber,
                            Status = group.Status,
                            CreatedOn = group.CreatedOn,
                            ActivePolicyCount = group.ActivePolicyCount,
                            Currency = group.CurrencyCode,
                            Balance = balance,
                            TotalPremium = group.TotalPremium,
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
