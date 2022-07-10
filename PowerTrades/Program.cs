using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using PowerTrades;
using Services;

var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

var configuration = builder.Build();

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Power Trade Export Service";
    })
    .ConfigureServices(services =>
    {
        LoggerProviderOptions.RegisterProviderOptions<EventLogSettings, EventLogLoggerProvider>(services);
        
        services.AddSingleton(configuration);
        services.AddTransient<IPowerTradeExportService, PowerTradeExportService>();
        services.AddTransient<IPowerService, PowerService>();
        services.AddHostedService<WindowsBackgroundService>();

    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));

        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();