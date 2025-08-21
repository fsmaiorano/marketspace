namespace BuildingBlocks.Loggers;

public sealed class ObservabilityOptions
{
    public string? ServiceName { get; set; }
    public string? ServiceVersion { get; set; }
    public string? OtlpEndpoint { get; set; }
    public bool EnableTracing { get; set; } = true;
    public bool EnableMetrics { get; set; } = true;
    public bool EnableLogExporter { get; set; } = true;
    public bool EnableSerilogConsole { get; set; } = true;
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    public bool EnableHttpClientInstrumentation { get; set; } = true;
    public bool EnablePrometheusExporter { get; set; } = false;
    public int PrometheusPort { get; set; } = 9464;
    public bool EnableJaegerExporter { get; set; } = false;
    public string? JaegerEndpoint { get; set; }
    public bool EnableLoki { get; set; } = false;
    public string? LokiUrl { get; set; }
}