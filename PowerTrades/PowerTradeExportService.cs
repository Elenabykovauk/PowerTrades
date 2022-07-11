using System.Globalization;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Services;

[assembly: InternalsVisibleTo("TestProject")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleToAttribute("TestProject")]
namespace PowerTrades
{
    public class PowerTradeExportService : IPowerTradeExportService
    {
        private readonly IPowerService _powerService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PowerTradeExportService> _logger;

        public PowerTradeExportService(
            IPowerService powerService,
            IConfiguration configuration,
            ILogger<PowerTradeExportService> logger) =>
                (_powerService, _configuration, _logger) = (powerService, configuration, logger);

        public async Task GetFileReportAsync(CancellationToken stoppingToken = default)
        {
            int.TryParse(_configuration["attemptsToRetry"], out int attempts);
            var trades = await GetTrades();
            var resultFilePath = $"{_configuration["resultFilePath"]}\\PowerPosition_{DateTime.Now.ToString("yyyMMdd_hhmm")}.csv";

            while (trades == null && attempts > 0)
            {
                trades = await GetTrades();
                attempts--;
            }

            if (trades == null)
            {
                _logger.LogInformation($"Number of attempts exceeded.");
                return;
            }

            try
            {
                var report = GetReport(trades);

                SaveCsv(resultFilePath, report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Report generation and saving error.");
            }
        }

        private IEnumerable<IntraDayReport> GetReport(IEnumerable<PowerTrade> trades)
        {
            DateTime timeStart = DateTime.Now.Date.AddHours(-2);

            return trades.SelectMany(x => x.Periods)
                      .GroupBy(x => x.Period)
                      .Select(group => new IntraDayReport
                      {
                          LocalTime = timeStart.AddHours(group.Key).ToShortTimeString(),
                          Volume = group.Sum(info => info.Volume)
                      });
        }

        private async Task<IEnumerable<PowerTrade>> GetTrades()
        {
            try
            {
                return await _powerService.GetTradesAsync(DateTime.Now.Date);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PowerService returned an error. {ex.Message}");
            }
            return null;
        }

        private void SaveCsv(string filePath, IEnumerable<IntraDayReport> list)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ","
            };
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(list);
            }
        }
    }
}
