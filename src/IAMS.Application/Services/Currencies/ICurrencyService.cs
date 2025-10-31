using IAMS.Application.DTOs.Currency;
using IAMS.Application.Models;

namespace IAMS.Application.Services.Currencies
{
    public interface ICurrencyService
    {
        // Currency Management
        Task<Result<List<CurrencyDto>>> GetAllCurrenciesAsync();
        Task<Result<List<CurrencyDto>>> GetActiveCurrenciesAsync();
        Task<Result<CurrencyDto>> GetByIdAsync(int id);
        Task<Result<CurrencyDto>> GetByCodeAsync(string code);
        Task<Result<CurrencyDto>> GetBaseCurrencyAsync();

        // Exchange Rates
        Task<Result<decimal>> GetExchangeRateAsync(int fromCurrencyId, int toCurrencyId, DateTime? date = null);
        Task<Result<decimal>> GetExchangeRateAsync(string fromCode, string toCode, DateTime? date = null);
        Task<Result<ExchangeRateDto>> GetLatestExchangeRateAsync(int fromCurrencyId, int toCurrencyId);
        Task<Result<List<ExchangeRateDto>>> GetExchangeRateHistoryAsync(int fromCurrencyId, int toCurrencyId, DateTime startDate, DateTime endDate);

        // Set Exchange Rates
        Task<Result<ExchangeRateDto>> SetExchangeRateAsync(CreateExchangeRateDto dto);
        Task<Result<List<ExchangeRateDto>>> SetExchangeRatesAsync(List<CreateExchangeRateDto> rates);
        Task<Result> UpdateExchangeRatesFromTCMBAsync(); // Turkish Central Bank API

        // Currency Conversion
        Task<Result<decimal>> ConvertAmountAsync(decimal amount, int fromCurrencyId, int toCurrencyId, DateTime? date = null);
        Task<Result<decimal>> ConvertAmountAsync(decimal amount, string fromCode, string toCode, DateTime? date = null);
        Task<Result<decimal>> ConvertToBaseCurrencyAsync(decimal amount, int fromCurrencyId, DateTime? date = null);
        Task<Result<MoneyDto>> ConvertMoney(MoneyDto money, int toCurrencyId, DateTime? date = null);

        // Batch Operations
        Task<Result<Dictionary<string, decimal>>> GetAllExchangeRatesToBaseAsync(DateTime? date = null);
        Task<Result<List<CurrencyConversionDto>>> ConvertBatchAsync(List<MoneyDto> amounts, int toCurrencyId);
    }
}