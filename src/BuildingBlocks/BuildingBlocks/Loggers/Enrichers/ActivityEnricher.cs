using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace BuildingBlocks.Loggers.Enrichers;

public sealed class ActivityEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Activity? activity = Activity.Current;
        if (activity == null) return;
        if (activity.TraceId != default)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
        if (activity.SpanId != default)
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
    }
}