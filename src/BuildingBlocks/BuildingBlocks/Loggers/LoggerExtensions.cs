using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Loggers;

public static class LoggerExtensions
{
    public static WebApplicationBuilder AddCustomLoggers(this WebApplicationBuilder builder)
    {
        string serviceName = builder.Environment.ApplicationName;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithProperty("LogType", "System")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] [CID:{CorrelationId}] [{LogType}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();

        builder.Services.AddSingleton(Log.Logger);
        builder.Services.AddLogging(b =>
        {
            b.ClearProviders();
            b.AddSerilog(dispose: true);
        });
        builder.Services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

        builder.Host.UseSerilog();

        return builder;
    }
}