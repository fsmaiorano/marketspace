using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using BuildingBlocks.Loggers.Enrichers;
using BuildingBlocks.Services.Correlation;
using Serilog.Sinks.Grafana.Loki;
using Prometheus;

namespace BuildingBlocks.Loggers;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration,
        Action<ObservabilityOptions>? configure = null)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationIdService, CorrelationIdService>();

        ObservabilityOptions opts = new ObservabilityOptions();
        configuration.GetSection("Observability").Bind(opts);
        configure?.Invoke(opts);

        opts.ServiceName ??= "UnknownService";

        ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(opts.ServiceName, serviceVersion: opts.ServiceVersion)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        // OpenTelemetry Tracing + Metrics
        services.AddOpenTelemetry()
            .WithTracing(b =>
            {
                if (!opts.EnableTracing) return;
                b.SetResourceBuilder(resourceBuilder)
                    .AddSource(opts.ServiceName!);
                if (opts.EnableAspNetCoreInstrumentation) b.AddAspNetCoreInstrumentation();
                if (opts.EnableHttpClientInstrumentation) b.AddHttpClientInstrumentation();
                if (!string.IsNullOrWhiteSpace(opts.OtlpEndpoint))
                    b.AddOtlpExporter(o => o.Endpoint = new Uri(opts.OtlpEndpoint!));
                if (opts.EnableJaegerExporter)
                {
                    b.AddJaegerExporter();
                }
            })
            .WithMetrics(b =>
            {
                if (!opts.EnableMetrics) return;
                b.SetResourceBuilder(resourceBuilder)
                    .AddRuntimeInstrumentation();
                if (opts.EnableAspNetCoreInstrumentation) b.AddAspNetCoreInstrumentation();
                if (!string.IsNullOrWhiteSpace(opts.OtlpEndpoint))
                    b.AddOtlpExporter(o => o.Endpoint = new Uri(opts.OtlpEndpoint!));
            });

        // Prometheus metrics (using prometheus-net)
        if (opts.EnablePrometheusExporter)
        {
            services.AddSingleton(Metrics.DefaultRegistry);
        }

        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new ActivityEnricher())
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName();

        loggerConfig.ReadFrom.Configuration(configuration);

        if (opts.EnableSerilogConsole)
            loggerConfig.WriteTo.Console(new CompactJsonFormatter());
        
        if (opts.EnableLoki && !string.IsNullOrWhiteSpace(opts.LokiUrl))
            loggerConfig.WriteTo.GrafanaLoki(opts.LokiUrl!);

        Log.Logger = loggerConfig.CreateLogger();

        services.AddSingleton(Log.Logger);
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        return services;
    }

    public static WebApplication UseObservability(this WebApplication app)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;
        app.UseSerilogRequestLogging();

        // Add Prometheus metrics endpoint if enabled
        var observabilityOptions = new ObservabilityOptions();
        app.Configuration.GetSection("Observability").Bind(observabilityOptions);
        if (observabilityOptions.EnablePrometheusExporter)
        {
            app.UseHttpMetrics();
            app.MapMetrics();
        }

        return app;
    }
}