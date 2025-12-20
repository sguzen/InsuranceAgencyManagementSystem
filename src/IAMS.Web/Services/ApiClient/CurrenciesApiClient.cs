using IAMS.Application.DTOs.Currency;
using IAMS.Shared.Models;

namespace IAMS.Web.Services.ApiClient
{
    public interface ICurrenciesApiClient
    {
        Task<Result<List<CurrencyDto>>> GetCurrenciesAsync();
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
    }
}
