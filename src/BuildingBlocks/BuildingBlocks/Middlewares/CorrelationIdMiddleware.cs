using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Serilog.Context;
using System.Diagnostics;
using BuildingBlocks.Services.Correlation;

namespace BuildingBlocks.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, ICorrelationIdService correlationIdService)
    {
        string correlationId;

        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues correlationIdHeader))
        {
            correlationId = Guid.CreateVersion7().ToString();
            context.Request.Headers[CorrelationIdHeader] = correlationId;
        }
        else
        {
            correlationId = correlationIdHeader.ToString();
        }

        correlationIdService.SetCorrelationId(correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        Activity? activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag("correlation.id", correlationId);
            activity.AddBaggage("CorrelationId", correlationId);
        }

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}