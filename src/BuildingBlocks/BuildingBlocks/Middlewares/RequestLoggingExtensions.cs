using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace BuildingBlocks.Middlewares;

public static class RequestLoggingExtensions
{
    /// <summary>
    /// Adds Serilog request logging with CorrelationId and RemoteIpAddress enrichment.
    /// Place after <see cref="CorrelationIdMiddleware"/> in the pipeline.
    /// </summary>
    public static IApplicationBuilder UseStructuredRequestLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagCtx, httpCtx) =>
            {
                diagCtx.Set("CorrelationId", httpCtx.Request.Headers["X-Correlation-ID"].ToString());
                diagCtx.Set("RemoteIpAddress", httpCtx.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            };
        });
    }
}
