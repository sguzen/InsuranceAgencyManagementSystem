using AutoMapper;
using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetExchangeRateHistory
{
    public class GetExchangeRateHistoryQueryHandler : IRequestHandler<GetExchangeRateHistoryQuery, Result<List<ExchangeRateDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetExchangeRateHistoryQueryHandler> _logger;

        public GetExchangeRateHistoryQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetExchangeRateHistoryQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<List<ExchangeRateDto>>> Handle(GetExchangeRateHistoryQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var exchangeRates = await _unitOfWork.CurrencyExchangeRates.GetAllAsync();
                var history = exchangeRates
                    .Where(er => er.FromCurrencyId == request.FromCurrencyId
                                 && er.ToCurrencyId == request.ToCurrencyId
                                 && er.EffectiveDate >= request.StartDate
                                 && er.EffectiveDate <= request.EndDate)
                    .OrderByDescending(er => er.EffectiveDate)
                    .ToList();

                var historyDtos = _mapper.Map<List<ExchangeRateDto>>(history);
                return Result<List<ExchangeRateDto>>.Success(historyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate history from {From} to {To}", request.FromCurrencyId, request.ToCurrencyId);
                return Result<List<ExchangeRateDto>>.InternalError("Error retrieving exchange rate history", new List<string> { ex.Message });
            }
        }
    }
}
