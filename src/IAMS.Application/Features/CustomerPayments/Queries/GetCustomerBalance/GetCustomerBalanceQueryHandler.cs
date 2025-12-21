using IAMS.Shared.DTOs.Payment;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.CustomerPayments.Queries.GetCustomerBalance
{
    public class GetCustomerBalanceQueryHandler
        : IRequestHandler<GetCustomerBalanceQuery, Result<CustomerBalanceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCustomerBalanceQueryHandler> _logger;

        public GetCustomerBalanceQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCustomerBalanceQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<CustomerBalanceDto>> Handle(
            GetCustomerBalanceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get customer
                var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId);
                if (customer == null)
                {
                    return Result<CustomerBalanceDto>.Failure("Müşteri bulunamadı", (List<string>?)null);
                }

                // Get multi-currency balance
                var balances = await _unitOfWork.CustomerPayments.GetCustomerBalanceAsync(request.CustomerId);

                var result = new CustomerBalanceDto
                {
                    CustomerId = customer.Id,
                    CustomerName = $"{customer.FirstName} {customer.LastName}",
                    Balances = new List<CurrencyBalanceDto>()
                };

                // Build currency balance DTOs
                foreach (var (currencyId, (totalDebt, totalPaid, balance)) in balances)
                {
                    var currency = await _unitOfWork.Currencies.GetByIdAsync(currencyId);
                    if (currency == null) continue;

                    result.Balances.Add(new CurrencyBalanceDto
                    {
                        CurrencyId = currencyId,
                        CurrencyCode = currency.Code,
                        CurrencySymbol = currency.Symbol,
                        CurrencyName = currency.Name ?? currency.Code,
                        TotalDebt = totalDebt,
                        TotalPaid = totalPaid,
                        Balance = balance
                    });
                }

                return Result<CustomerBalanceDto>.Success(result, "Müşteri bakiyesi getirildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer balance for customer {CustomerId}", request.CustomerId);
                return Result<CustomerBalanceDto>.InternalError(
                    "Müşteri bakiyesi alınırken bir hata oluştu",
                    new List<string> { ex.Message });
            }
        }
    }
}
