using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;
using System.Diagnostics;

namespace BuildingBlocks.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdItemKey = "CorrelationId"; // unify with service expectation

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationId))
        {
            correlationId = Guid.CreateVersion7().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Store under both the header key and simplified key for service retrieval
        context.Items[CorrelationIdHeader] = correlationId;
        context.Items[CorrelationIdItemKey] = correlationId.ToString();

        // Propagate into current activity (trace) if present
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("correlation.id", correlationId.ToString());
            activity.AddBaggage("CorrelationId", correlationId.ToString());
        }

        // Push CorrelationId into Serilog LogContext for automatic enrichment
        using (LogContext.PushProperty("CorrelationId", correlationId.ToString()))
        {
            await next(context);
        }
    }
}