using IAMS.Application.Services.Currencies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IAMS.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service that updates exchange rates daily from TCMB
    /// </summary>
    public class ExchangeRateUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExchangeRateUpdateService> _logger;
        private readonly TimeSpan _updateInterval = TimeSpan.FromHours(24);
        private readonly TimeSpan _updateTime = new TimeSpan(9, 0, 0); // 09:00 AM

        public ExchangeRateUpdateService(
            IServiceProvider serviceProvider,
            ILogger<ExchangeRateUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Exchange Rate Update Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var nextUpdate = CalculateNextUpdateTime(now);
                    var delay = nextUpdate - now;

                    _logger.LogInformation("Next exchange rate update scheduled at {Time}", nextUpdate);
                    await Task.Delay(delay, stoppingToken);

                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await UpdateExchangeRatesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in exchange rate update service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Retry in 1 hour
                }
            }
        }

        private DateTime CalculateNextUpdateTime(DateTime now)
        {
            var today = now.Date + _updateTime;

            // If already past update time today, schedule for tomorrow
            if (now > today)
            {
                return today.AddDays(1);
            }

            return today;
        }

        private async Task UpdateExchangeRatesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();

            try
            {
                _logger.LogInformation("Starting exchange rate update from TCMB");
                var result = await currencyService.UpdateExchangeRatesFromTCMBAsync();

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Exchange rates updated successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to update exchange rates: {Message}", result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exchange rates");
            }
        }
    }
}