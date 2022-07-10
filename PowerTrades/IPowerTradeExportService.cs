
namespace PowerTrades
{
    internal interface IPowerTradeExportService
    {
        Task GetFileReportAsync(CancellationToken stoppingToken = default);
    }
}