using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace IAMS.Infrastructure.Services
{
    /// <summary>
    /// Service to fetch exchange rates from Turkish Central Bank (TCMB)
    /// </summary>
    public interface ITCMBService
    {
        Task<Dictionary<string, decimal>> GetLatestExchangeRatesAsync();
        Task<Dictionary<string, decimal>> GetExchangeRatesForDateAsync(DateTime date);
    }

    public class TCMBService : ITCMBService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TCMBService> _logger;
        private const string TCMB_URL = "https://www.tcmb.gov.tr/kurlar/today.xml";
        private const string TCMB_DATE_URL = "https://www.tcmb.gov.tr/kurlar/{0}/{1}.xml";

        public TCMBService(HttpClient httpClient, ILogger<TCMBService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Dictionary<string, decimal>> GetLatestExchangeRatesAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(TCMB_URL);
                return ParseXmlResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest exchange rates from TCMB");
                throw;
            }
        }

        public async Task<Dictionary<string, decimal>> GetExchangeRatesForDateAsync(DateTime date)
        {
            try
            {
                var yearMonth = date.ToString("yyyyMM");
                var dayMonth = date.ToString("ddMMyyyy");
                var url = string.Format(TCMB_DATE_URL, yearMonth, dayMonth);

                var response = await _httpClient.GetStringAsync(url);
                return ParseXmlResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exchange rates for date {Date} from TCMB", date);
                throw;
            }
        }

        private Dictionary<string, decimal> ParseXmlResponse(string xml)
        {
            var rates = new Dictionary<string, decimal>();
            var doc = XDocument.Parse(xml);

            foreach (var currency in doc.Descendants("Currency"))
            {
                var code = currency.Attribute("CurrencyCode")?.Value;
                var forexSellingStr = currency.Element("ForexSelling")?.Value;

                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(forexSellingStr))
                {
                    if (decimal.TryParse(forexSellingStr.Replace(',', '.'),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal rate))
                    {
                        rates[code] = rate;
                    }
                }
            }

            _logger.LogInformation("Parsed {Count} exchange rates from TCMB", rates.Count);
            return rates;
        }
    }
}