namespace BuildingBlocks.Loggers;

/// <summary>
/// Options controlling Serilog + OpenTelemetry setup.
/// </summary>
public sealed class ObservabilityOptions
{
    /// <summary>Name of the logical service (defaults to ApplicationName).</summary>
    public string? ServiceName { get; set; }
    /// <summary>Version of the service (optional).</summary>
    public string? ServiceVersion { get; set; }
    /// <summary>OTLP endpoint (ex: http://otel-collector:4317). If null, exporter not added.</summary>
    public string? OtlpEndpoint { get; set; }
    /// <summary>Enable tracing (default true).</summary>
    public bool EnableTracing { get; set; } = true;
    /// <summary>Enable metrics (default true).</summary>
    public bool EnableMetrics { get; set; } = true;
    /// <summary>Enable OpenTelemetry log exporter (default true).</summary>
    public bool EnableLogExporter { get; set; } = true;
    /// <summary>Enable Serilog console sink (default true).</summary>
    public bool EnableSerilogConsole { get; set; } = true;
    /// <summary>Include HTTP request payload instrumentation options (aspnetcore) - default true.</summary>
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    /// <summary>Include HttpClient instrumentation (default true).</summary>
    public bool EnableHttpClientInstrumentation { get; set; } = true;
    /// <summary>Enable Prometheus exporter for metrics (default false).</summary>
    public bool EnablePrometheusExporter { get; set; } = false;
    /// <summary>Prometheus metrics endpoint port (default 9464).</summary>
    public int PrometheusPort { get; set; } = 9464;
    /// <summary>Enable Jaeger exporter for tracing (default false).</summary>
    public bool EnableJaegerExporter { get; set; } = false;
    /// <summary>Jaeger endpoint (ex: http://jaeger:14268/api/traces). If null, default is used.</summary>
    public string? JaegerEndpoint { get; set; }
    /// <summary>Enable Loki sink for logs (default false).</summary>
    public bool EnableLoki { get; set; } = false;
    /// <summary>Loki URL (ex: http://loki:3100). If null, Loki sink is not added.</summary>
    public string? LokiUrl { get; set; }
}
