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
using Serilog.Formatting;
using BuildingBlocks.Loggers.Enrichers;
using BuildingBlocks.Services.Correlation;
using Serilog.Sinks.Grafana.Loki;
using Prometheus;
using System.Text;

namespace BuildingBlocks.Loggers;

public class LokiJsonFormatter : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        StringBuilder json = new StringBuilder();
        json.Append("{");

        json.Append($"\"timestamp\":\"{logEvent.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}\",");

        json.Append($"\"level\":\"{logEvent.Level}\",");

        string renderedMessage = logEvent.RenderMessage();
        json.Append($"\"message\":\"{EscapeJson(renderedMessage)}\",");

        json.Append("\"properties\":{");
        int propertyCount = 0;
        foreach (KeyValuePair<string, LogEventPropertyValue> property in logEvent.Properties)
        {
            if (propertyCount > 0) json.Append(",");
            json.Append($"\"{property.Key}\":");

            if (property.Value is ScalarValue scalarValue)
            {
                json.Append($"\"{EscapeJson(scalarValue.Value?.ToString() ?? "null")}\"");
            }
            else
            {
                json.Append($"\"{EscapeJson(property.Value.ToString())}\"");
            }

            propertyCount++;
        }

        json.Append("}");

        if (logEvent.Exception != null)
        {
            json.Append($",\"exception\":\"{EscapeJson(logEvent.Exception.ToString())}\"");
        }

        json.Append("}");
        output.Write(json.ToString());
    }

    private static string EscapeJson(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        return text.Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}

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
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] [{ServiceName}] [CorrelationId: {CorrelationId}] {Message:lj} {Properties:j}{NewLine}{Exception}",
                restrictedToMinimumLevel: LogEventLevel.Information
            );
        }
        
        if (opts.EnableLoki && !string.IsNullOrWhiteSpace(opts.LokiUrl))
        {
            List<LokiLabel> lokiLabels =
            [
                new() { Key = "service", Value = opts.ServiceName },
                new()
                {
                    Key = "environment",
                    Value = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                },

                new() { Key = "version", Value = opts.ServiceVersion ?? "1.0.0" }
            ];

            loggerConfig.WriteTo.GrafanaLoki(
                uri: opts.LokiUrl,
                labels: lokiLabels,
                restrictedToMinimumLevel: LogEventLevel.Information,
                textFormatter: new LokiJsonFormatter()
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
        ObservabilityOptions observabilityOptions = new ObservabilityOptions();
        app.Configuration.GetSection("Observability").Bind(observabilityOptions);

        if (!observabilityOptions.EnablePrometheusExporter)
            return app;

        app.UseHttpMetrics();
        app.MapMetrics();

        return app;
    }
}