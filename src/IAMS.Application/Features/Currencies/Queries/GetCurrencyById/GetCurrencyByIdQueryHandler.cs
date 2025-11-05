using AutoMapper;
using IAMS.Application.DTOs.Currency;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMS.Application.Features.Currencies.Queries.GetCurrencyById
{
    public class GetCurrencyByIdQueryHandler : IRequestHandler<GetCurrencyByIdQuery, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCurrencyByIdQueryHandler> _logger;

        public GetCurrencyByIdQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCurrencyByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CurrencyDto>> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByIdAsync(request.Id);
                if (currency == null)
                {
                    return Result<CurrencyDto>.NotFound($"Currency with ID {request.Id} not found");
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Result<CurrencyDto>.Success(currencyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with ID: {Id}", request.Id);
                return Result<CurrencyDto>.InternalError("Error retrieving currency", new List<string> { ex.Message });
            }
        }
    }
}
