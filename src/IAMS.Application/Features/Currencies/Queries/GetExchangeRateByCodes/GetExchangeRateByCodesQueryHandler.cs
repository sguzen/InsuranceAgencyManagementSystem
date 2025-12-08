using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetExchangeRateByCodes
{
    public class GetExchangeRateByCodesQueryHandler : IRequestHandler<GetExchangeRateByCodesQuery, Result<decimal>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetExchangeRateByCodesQueryHandler> _logger;

        public GetExchangeRateByCodesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetExchangeRateByCodesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<decimal>> Handle(GetExchangeRateByCodesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // If same currency, rate is 1
                if (request.FromCode.Equals(request.ToCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<decimal>.Success(1.0m);
                }

                var currencies = await _unitOfWork.Currencies.GetAllAsync();
                var fromCurrency = currencies.FirstOrDefault(c => c.Code.Equals(request.FromCode, StringComparison.OrdinalIgnoreCase));
                var toCurrency = currencies.FirstOrDefault(c => c.Code.Equals(request.ToCode, StringComparison.OrdinalIgnoreCase));

                if (fromCurrency == null)
                {
                    return Result<decimal>.NotFound($"Currency with code '{request.FromCode}' not found");
                }

                if (toCurrency == null)
                {
                    return Result<decimal>.NotFound($"Currency with code '{request.ToCode}' not found");
                }

                var targetDate = request.Date ?? DateTime.UtcNow;
                var exchangeRates = await _unitOfWork.CurrencyExchangeRates.GetAllAsync();

                var rate = exchangeRates
                    .Where(er => er.FromCurrencyId == fromCurrency.Id
                                 && er.ToCurrencyId == toCurrency.Id
                                 && er.EffectiveDate <= targetDate
                                 && (er.ExpiryDate == null || er.ExpiryDate >= targetDate)
                                 && er.IsActive)
                    .OrderByDescending(er => er.EffectiveDate)
                    .FirstOrDefault();

                if (rate == null)
                {
                    // Try inverse rate
                    var inverseRate = exchangeRates
                        .Where(er => er.FromCurrencyId == toCurrency.Id
                                     && er.ToCurrencyId == fromCurrency.Id
                                     && er.EffectiveDate <= targetDate
                                     && (er.ExpiryDate == null || er.ExpiryDate >= targetDate)
                                     && er.IsActive)
                        .OrderByDescending(er => er.EffectiveDate)
                        .FirstOrDefault();

                    if (inverseRate != null)
                    {
                        return Result<decimal>.Success(1 / inverseRate.Rate);
                    }

                    return Result<decimal>.NotFound($"Exchange rate not found for {request.FromCode} to {request.ToCode}");
                }

                return Result<decimal>.Success(rate.Rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate from {From} to {To}", request.FromCode, request.ToCode);
                return Result<decimal>.InternalError("Error retrieving exchange rate", new List<string> { ex.Message });
            }
        }
    }
}
