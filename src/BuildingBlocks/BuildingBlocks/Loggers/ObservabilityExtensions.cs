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

        // Ensure service name is set to prevent unknown_services in Loki
        opts.ServiceName ??= "UnknownService";

        ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(opts.ServiceName!, serviceVersion: opts.ServiceVersion)
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
            .MinimumLevel.Information() // Set minimum level to Information to remove Trace logs
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new ActivityEnricher())
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("ServiceName", opts.ServiceName)
            .Enrich.WithProperty("ServiceVersion", opts.ServiceVersion ?? "1.0.0");

        // Don't read from configuration to avoid conflicts - we'll configure everything here
        // loggerConfig.ReadFrom.Configuration(configuration);

        if (opts.EnableSerilogConsole)
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] [CorrelationId: {CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information
            );
        }
        
        if (opts.EnableLoki && !string.IsNullOrWhiteSpace(opts.LokiUrl))
        {
            var lokiLabels = new List<LokiLabel>
            {
                new() { Key = "service", Value = opts.ServiceName },
                new() { Key = "environment", Value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production" },
                new() { Key = "version", Value = opts.ServiceVersion ?? "1.0.0" }
            };

            loggerConfig.WriteTo.GrafanaLoki(
                uri: opts.LokiUrl,
                labels: lokiLabels,
                restrictedToMinimumLevel: LogEventLevel.Information,
                textFormatter: new CompactJsonFormatter()
            );
        }

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