using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PowerTrades
{
    internal class WindowsBackgroundService : BackgroundService
    {
        private readonly IPowerTradeExportService _powerTradeExportService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(
            IPowerTradeExportService powerTradeExportService,
            IConfiguration configuration,
            ILogger<WindowsBackgroundService> logger) =>
                (_powerTradeExportService, _configuration, _logger) = (powerTradeExportService, configuration, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int.TryParse(_configuration["serviceInterval"], out int serviceInterval);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"PowerTradeExportService starting at {DateTime.Now}.");
                    await _powerTradeExportService.GetFileReportAsync(stoppingToken);
                    _logger.LogInformation($"PowerTradeExportService finished at {DateTime.Now}.");

                    await Task.Delay(TimeSpan.FromMinutes(serviceInterval), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background Service error", ex.Message);

                Environment.Exit(1);
            }
        }
    }
}