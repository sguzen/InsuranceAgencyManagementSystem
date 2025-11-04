using AutoMapper;
using IAMS.Application.DTOs.Currency;
using IAMS.Application.Interfaces.Repositories;
using IAMS.Application.Models;
using IAMS.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Xml.Linq;

namespace IAMS.Application.Services.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public CurrencyService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CurrencyService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        #region Currency Management

        public async Task<Result<List<CurrencyDto>>> GetAllCurrenciesAsync()
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetAllAsync();
                var currencyDtos = _mapper.Map<List<CurrencyDto>>(currencies);
                return Result<List<CurrencyDto>>.Success(currencyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all currencies");
                return Result<List<CurrencyDto>>.InternalError("Error retrieving currencies", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<CurrencyDto>>> GetActiveCurrenciesAsync()
        {
            try
            {
                var currencies = await _unitOfWork.Currencies.GetActiveCurrenciesAsync();
                var currencyDtos = _mapper.Map<List<CurrencyDto>>(currencies);
                return Result<List<CurrencyDto>>.Success(currencyDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active currencies");
                return Result<List<CurrencyDto>>.InternalError("Error retrieving active currencies", new List<string> { ex.Message });
            }
        }

        public async Task<Result<CurrencyDto>> GetByIdAsync(int id)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByIdAsync(id);
                if (currency == null)
                {
                    return Result<CurrencyDto>.NotFound($"Currency with ID {id} not found");
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Result<CurrencyDto>.Success(currencyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with ID: {CurrencyId}", id);
                return Result<CurrencyDto>.InternalError("Error retrieving currency", new List<string> { ex.Message });
            }
        }

        public async Task<Result<CurrencyDto>> GetByCodeAsync(string code)
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetByCodeAsync(code);
                if (currency == null)
                {
                    return Result<CurrencyDto>.NotFound($"Currency with code {code} not found");
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Result<CurrencyDto>.Success(currencyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving currency with code: {CurrencyCode}", code);
                return Result<CurrencyDto>.InternalError("Error retrieving currency", new List<string> { ex.Message });
            }
        }

        public async Task<Result<CurrencyDto>> GetBaseCurrencyAsync()
        {
            try
            {
                var currency = await _unitOfWork.Currencies.GetBaseCurrencyAsync();
                if (currency == null)
                {
                    return Result<CurrencyDto>.NotFound("Base currency not found");
                }

                var currencyDto = _mapper.Map<CurrencyDto>(currency);
                return Result<CurrencyDto>.Success(currencyDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving base currency");
                return Result<CurrencyDto>.InternalError("Error retrieving base currency", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Exchange Rates

        public async Task<Result<decimal>> GetExchangeRateAsync(int fromCurrencyId, int toCurrencyId, DateTime? date = null)
        {
            try
            {
                // If currencies are the same, return 1
                if (fromCurrencyId == toCurrencyId)
                {
                    return Result<decimal>.Success(1.0m);
                }

                CurrencyExchangeRate? exchangeRate;

                if (date.HasValue)
                {
                    exchangeRate = await _unitOfWork.CurrencyExchangeRates.GetRateByDateAsync(fromCurrencyId, toCurrencyId, date.Value);
                }
                else
                {
                    exchangeRate = await _unitOfWork.CurrencyExchangeRates.GetLatestRateAsync(fromCurrencyId, toCurrencyId);
                }

                if (exchangeRate == null)
                {
                    return Result<decimal>.NotFound($"Exchange rate not found for the specified currencies{(date.HasValue ? $" on {date.Value:yyyy-MM-dd}" : "")}");
                }

                return Result<decimal>.Success(exchangeRate.Rate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate from currency {FromId} to {ToId}", fromCurrencyId, toCurrencyId);
                return Result<decimal>.InternalError("Error retrieving exchange rate", new List<string> { ex.Message });
            }
        }

        public async Task<Result<decimal>> GetExchangeRateAsync(string fromCode, string toCode, DateTime? date = null)
        {
            try
            {
                // If currencies are the same, return 1
                if (fromCode.Equals(toCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<decimal>.Success(1.0m);
                }

                var fromCurrency = await _unitOfWork.Currencies.GetByCodeAsync(fromCode);
                if (fromCurrency == null)
                {
                    return Result<decimal>.NotFound($"Currency with code {fromCode} not found");
                }

                var toCurrency = await _unitOfWork.Currencies.GetByCodeAsync(toCode);
                if (toCurrency == null)
                {
                    return Result<decimal>.NotFound($"Currency with code {toCode} not found");
                }

                return await GetExchangeRateAsync(fromCurrency.Id, toCurrency.Id, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate from {FromCode} to {ToCode}", fromCode, toCode);
                return Result<decimal>.InternalError("Error retrieving exchange rate", new List<string> { ex.Message });
            }
        }

        public async Task<Result<ExchangeRateDto>> GetLatestExchangeRateAsync(int fromCurrencyId, int toCurrencyId)
        {
            try
            {
                var exchangeRate = await _unitOfWork.CurrencyExchangeRates.GetLatestRateAsync(fromCurrencyId, toCurrencyId);
                if (exchangeRate == null)
                {
                    return Result<ExchangeRateDto>.NotFound("Exchange rate not found");
                }

                var exchangeRateDto = _mapper.Map<ExchangeRateDto>(exchangeRate);
                return Result<ExchangeRateDto>.Success(exchangeRateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest exchange rate from currency {FromId} to {ToId}", fromCurrencyId, toCurrencyId);
                return Result<ExchangeRateDto>.InternalError("Error retrieving exchange rate", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<ExchangeRateDto>>> GetExchangeRateHistoryAsync(int fromCurrencyId, int toCurrencyId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var exchangeRates = await _unitOfWork.CurrencyExchangeRates.GetRateHistoryAsync(fromCurrencyId, toCurrencyId, startDate, endDate);
                var exchangeRateDtos = _mapper.Map<List<ExchangeRateDto>>(exchangeRates);
                return Result<List<ExchangeRateDto>>.Success(exchangeRateDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exchange rate history from currency {FromId} to {ToId}", fromCurrencyId, toCurrencyId);
                return Result<List<ExchangeRateDto>>.InternalError("Error retrieving exchange rate history", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Set Exchange Rates

        public async Task<Result<ExchangeRateDto>> SetExchangeRateAsync(CreateExchangeRateDto dto)
        {
            try
            {
                // Validate currencies exist
                var fromCurrency = await _unitOfWork.Currencies.GetByIdAsync(dto.FromCurrencyId);
                if (fromCurrency == null)
                {
                    return Result<ExchangeRateDto>.NotFound($"From currency with ID {dto.FromCurrencyId} not found");
                }

                var toCurrency = await _unitOfWork.Currencies.GetByIdAsync(dto.ToCurrencyId);
                if (toCurrency == null)
                {
                    return Result<ExchangeRateDto>.NotFound($"To currency with ID {dto.ToCurrencyId} not found");
                }

                // Create exchange rate
                var exchangeRate = _mapper.Map<CurrencyExchangeRate>(dto);
                exchangeRate.CreatedOn = DateTime.UtcNow;

                await _unitOfWork.CurrencyExchangeRates.AddAsync(exchangeRate);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Exchange rate created from {FromCode} to {ToCode} with rate {Rate}",
                    fromCurrency.Code, toCurrency.Code, dto.Rate);

                var exchangeRateDto = _mapper.Map<ExchangeRateDto>(exchangeRate);
                return Result<ExchangeRateDto>.Success(exchangeRateDto, "Exchange rate set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting exchange rate");
                return Result<ExchangeRateDto>.InternalError("Error setting exchange rate", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<ExchangeRateDto>>> SetExchangeRatesAsync(List<CreateExchangeRateDto> rates)
        {
            try
            {
                var exchangeRateDtos = new List<ExchangeRateDto>();

                foreach (var dto in rates)
                {
                    var result = await SetExchangeRateAsync(dto);
                    if (result.IsSuccess && result.Data != null)
                    {
                        exchangeRateDtos.Add(result.Data);
                    }
                }

                return Result<List<ExchangeRateDto>>.Success(exchangeRateDtos, $"{exchangeRateDtos.Count} exchange rates set successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting multiple exchange rates");
                return Result<List<ExchangeRateDto>>.InternalError("Error setting exchange rates", new List<string> { ex.Message });
            }
        }

        public async Task<Result> UpdateExchangeRatesFromTCMBAsync()
        {
            try
            {
                _logger.LogInformation("Starting TCMB exchange rate update");

                // Get today's TCMB rates
                var tcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.GetAsync(tcmbUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return Result.Failure("Failed to fetch exchange rates from TCMB");
                }

                var xmlContent = await response.Content.ReadAsStringAsync();
                var xmlDoc = XDocument.Parse(xmlContent);

                // Get base currency (TRY)
                var baseCurrency = await _unitOfWork.Currencies.GetBaseCurrencyAsync();
                if (baseCurrency == null)
                {
                    return Result.Failure("Base currency (TRY) not found");
                }

                var ratesToCreate = new List<CreateExchangeRateDto>();
                var today = DateTime.UtcNow.Date;

                // Parse XML and extract rates
                var currencies = xmlDoc.Descendants("Currency");
                foreach (var currencyNode in currencies)
                {
                    var currencyCode = currencyNode.Attribute("CurrencyCode")?.Value;
                    var forexBuyingStr = currencyNode.Element("ForexBuying")?.Value;

                    if (string.IsNullOrEmpty(currencyCode) || string.IsNullOrEmpty(forexBuyingStr))
                        continue;

                    // Get currency from database
                    var currency = await _unitOfWork.Currencies.GetByCodeAsync(currencyCode);
                    if (currency == null)
                        continue;

                    if (decimal.TryParse(forexBuyingStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var rate))
                    {
                        // Create rate from foreign currency to TRY
                        ratesToCreate.Add(new CreateExchangeRateDto
                        {
                            FromCurrencyId = currency.Id,
                            ToCurrencyId = baseCurrency.Id,
                            Rate = rate,
                            EffectiveDate = today,
                            Source = "TCMB"
                        });

                        // Create reverse rate from TRY to foreign currency
                        ratesToCreate.Add(new CreateExchangeRateDto
                        {
                            FromCurrencyId = baseCurrency.Id,
                            ToCurrencyId = currency.Id,
                            Rate = 1 / rate,
                            EffectiveDate = today,
                            Source = "TCMB"
                        });
                    }
                }

                if (ratesToCreate.Any())
                {
                    await SetExchangeRatesAsync(ratesToCreate);
                    _logger.LogInformation("Successfully updated {Count} exchange rates from TCMB", ratesToCreate.Count);
                    return Result.Success($"Successfully updated {ratesToCreate.Count} exchange rates from TCMB");
                }

                return Result.Failure("No exchange rates were updated");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates from TCMB");
                return Result.InternalError("Error updating exchange rates from TCMB", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Currency Conversion

        public async Task<Result<decimal>> ConvertAmountAsync(decimal amount, int fromCurrencyId, int toCurrencyId, DateTime? date = null)
        {
            try
            {
                if (fromCurrencyId == toCurrencyId)
                {
                    return Result<decimal>.Success(amount);
                }

                var rateResult = await GetExchangeRateAsync(fromCurrencyId, toCurrencyId, date);
                if (!rateResult.IsSuccess)
                {
                    return Result<decimal>.Failure(rateResult.Message, rateResult.Errors);
                }

                var convertedAmount = amount * rateResult.Data;
                return Result<decimal>.Success(convertedAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting amount from currency {FromId} to {ToId}", fromCurrencyId, toCurrencyId);
                return Result<decimal>.InternalError("Error converting amount", new List<string> { ex.Message });
            }
        }

        public async Task<Result<decimal>> ConvertAmountAsync(decimal amount, string fromCode, string toCode, DateTime? date = null)
        {
            try
            {
                if (fromCode.Equals(toCode, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<decimal>.Success(amount);
                }

                var fromCurrency = await _unitOfWork.Currencies.GetByCodeAsync(fromCode);
                if (fromCurrency == null)
                {
                    return Result<decimal>.NotFound($"Currency with code {fromCode} not found");
                }

                var toCurrency = await _unitOfWork.Currencies.GetByCodeAsync(toCode);
                if (toCurrency == null)
                {
                    return Result<decimal>.NotFound($"Currency with code {toCode} not found");
                }

                return await ConvertAmountAsync(amount, fromCurrency.Id, toCurrency.Id, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting amount from {FromCode} to {ToCode}", fromCode, toCode);
                return Result<decimal>.InternalError("Error converting amount", new List<string> { ex.Message });
            }
        }

        public async Task<Result<decimal>> ConvertToBaseCurrencyAsync(decimal amount, int fromCurrencyId, DateTime? date = null)
        {
            try
            {
                var baseCurrency = await _unitOfWork.Currencies.GetBaseCurrencyAsync();
                if (baseCurrency == null)
                {
                    return Result<decimal>.NotFound("Base currency not found");
                }

                return await ConvertAmountAsync(amount, fromCurrencyId, baseCurrency.Id, date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting amount to base currency from currency {FromId}", fromCurrencyId);
                return Result<decimal>.InternalError("Error converting to base currency", new List<string> { ex.Message });
            }
        }

        public async Task<Result<MoneyDto>> ConvertMoney(MoneyDto money, int toCurrencyId, DateTime? date = null)
        {
            try
            {
                var convertedAmountResult = await ConvertAmountAsync(money.Amount, money.CurrencyId, toCurrencyId, date);
                if (!convertedAmountResult.IsSuccess)
                {
                    return Result<MoneyDto>.Failure(convertedAmountResult.Message, convertedAmountResult.Errors);
                }

                var toCurrency = await _unitOfWork.Currencies.GetByIdAsync(toCurrencyId);
                if (toCurrency == null)
                {
                    return Result<MoneyDto>.NotFound($"Currency with ID {toCurrencyId} not found");
                }

                var convertedMoney = new MoneyDto
                {
                    Amount = convertedAmountResult.Data,
                    CurrencyId = toCurrencyId,
                    CurrencyCode = toCurrency.Code
                };

                return Result<MoneyDto>.Success(convertedMoney);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting money to currency {ToId}", toCurrencyId);
                return Result<MoneyDto>.InternalError("Error converting money", new List<string> { ex.Message });
            }
        }

        #endregion

        #region Batch Operations

        public async Task<Result<Dictionary<string, decimal>>> GetAllExchangeRatesToBaseAsync(DateTime? date = null)
        {
            try
            {
                var baseCurrency = await _unitOfWork.Currencies.GetBaseCurrencyAsync();
                if (baseCurrency == null)
                {
                    return Result<Dictionary<string, decimal>>.NotFound("Base currency not found");
                }

                var currencies = await _unitOfWork.Currencies.GetActiveCurrenciesAsync();
                var rates = new Dictionary<string, decimal>();

                foreach (var currency in currencies)
                {
                    if (currency.Id == baseCurrency.Id)
                    {
                        rates[currency.Code] = 1.0m;
                        continue;
                    }

                    var rateResult = await GetExchangeRateAsync(currency.Id, baseCurrency.Id, date);
                    if (rateResult.IsSuccess)
                    {
                        rates[currency.Code] = rateResult.Data;
                    }
                }

                return Result<Dictionary<string, decimal>>.Success(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all exchange rates to base currency");
                return Result<Dictionary<string, decimal>>.InternalError("Error retrieving exchange rates", new List<string> { ex.Message });
            }
        }

        public async Task<Result<List<CurrencyConversionDto>>> ConvertBatchAsync(List<MoneyDto> amounts, int toCurrencyId)
        {
            try
            {
                var conversions = new List<CurrencyConversionDto>();

                foreach (var money in amounts)
                {
                    var convertedResult = await ConvertMoney(money, toCurrencyId);
                    if (convertedResult.IsSuccess && convertedResult.Data != null)
                    {
                        conversions.Add(new CurrencyConversionDto
                        {
                            OriginalAmount = money.Amount,
                            OriginalCurrencyId = money.CurrencyId,
                            ConvertedAmount = convertedResult.Data.Amount,
                            ConvertedCurrencyId = toCurrencyId
                        });
                    }
                }

                return Result<List<CurrencyConversionDto>>.Success(conversions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing batch currency conversion");
                return Result<List<CurrencyConversionDto>>.InternalError("Error performing batch conversion", new List<string> { ex.Message });
            }
        }

        #endregion
    }
}
