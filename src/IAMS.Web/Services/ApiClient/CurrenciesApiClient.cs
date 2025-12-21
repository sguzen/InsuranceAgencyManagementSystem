using IAMS.Shared.DTOs.Currency;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface ICurrenciesApiClient
    {
        Task<Result<List<CurrencyDto>>> GetCurrenciesAsync();
        Task<Result<List<CurrencyDto>>> GetActiveCurrenciesAsync();
    }

    public class CurrenciesApiClient : BaseApiClient, ICurrenciesApiClient
    {
        public CurrenciesApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<Result<List<CurrencyDto>>> GetCurrenciesAsync()
        {
            return await GetAsync<List<CurrencyDto>>("api/currencies");
        }

        public async Task<Result<List<CurrencyDto>>> GetActiveCurrenciesAsync()
        {
            // Note: api/currencies returns only active currencies by default
            return await GetAsync<List<CurrencyDto>>("api/currencies");
        }
    }
}
