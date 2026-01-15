using BuildingBlocks.Loggers.Abstractions;
using BuildingBlocks.Loggers.Enrichers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Loggers;

public static class LoggerExtensions
{
    public static IServiceCollection AddCustomLoggers(this IServiceCollection services)
    {
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Information() // Set minimum level to Information to remove Trace logs
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new ActivityEnricher())
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName();


        loggerConfig.WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] [CorrelationId: {CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information
        );

        Log.Logger = loggerConfig.CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        services.AddScoped(typeof(IApplicationLogger<>), typeof(ApplicationLogger<>));
        services.AddScoped(typeof(IBusinessLogger<>), typeof(BusinessLogger<>));

        return services;
    }
}