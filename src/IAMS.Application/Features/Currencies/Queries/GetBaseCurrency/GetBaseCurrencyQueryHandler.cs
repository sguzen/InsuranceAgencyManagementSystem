using AutoMapper;
using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetBaseCurrency
{
    public class GetBaseCurrencyQueryHandler : IRequestHandler<GetBaseCurrencyQuery, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetBaseCurrencyQueryHandler> _logger;

        public GetBaseCurrencyQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetBaseCurrencyQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CurrencyDto>> Handle(GetBaseCurrencyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetAllAsync();
                var baseCurrency = currencies.FirstOrDefault(c => c.IsBaseCurrency);

                if (baseCurrency == null)
                {
                    return Result<CurrencyDto>.NotFound("No base currency configured");
                }

                var currencyDto = _mapper.Map<CurrencyDto>(baseCurrency);
                return Result<CurrencyDto>.Success(currencyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving base currency");
                return Result<CurrencyDto>.InternalError("Error retrieving base currency", new List<string> { ex.Message });
            }
        }
    }
}
