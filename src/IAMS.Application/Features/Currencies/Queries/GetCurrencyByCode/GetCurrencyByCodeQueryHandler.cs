using AutoMapper;
using IAMS.Shared.DTOs.Currency;
using IAMS.Application.Models;
using IAMS.Shared.Interfaces.Repositories;
using IAMS.Shared.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetCurrencyByCode
{
    public class GetCurrencyByCodeQueryHandler : IRequestHandler<GetCurrencyByCodeQuery, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCurrencyByCodeQueryHandler> _logger;

        public GetCurrencyByCodeQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCurrencyByCodeQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CurrencyDto>> Handle(GetCurrencyByCodeQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetAllAsync();
                var currency = currencies.FirstOrDefault(c => c.Code.Equals(request.Code, StringComparison.OrdinalIgnoreCase));

                if (currency == null)
                {
                    return Result<CurrencyDto>.NotFound($"Currency with code '{request.Code}' not found");
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Result<CurrencyDto>.Success(currencyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with code: {Code}", request.Code);
                return Result<CurrencyDto>.InternalError("Error retrieving currency", new List<string> { ex.Message });
            }
        }
    }
}
