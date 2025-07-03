using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Text;

namespace ApiAggregator.Infrastructure.Startup;

public static class LoggingConfiguration
{
    public static void ConfigureLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        //Force UTF-8 console output
        Console.OutputEncoding = Encoding.UTF8;
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            //Console logging
            .WriteTo.Console(
                restrictedToMinimumLevel: LogEventLevel.Information)
            //File logging
            .WriteTo.File(
                path: "../logs/log.txt",
                outputTemplate: "[{Timestamp:u} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Warning)
            .CreateLogger();

        builder.ClearProviders();
        builder.AddSerilog(logger);
    }
}