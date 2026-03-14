using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Http;

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Stopwatch sw = Stopwatch.StartNew();
        try
        {
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            sw.Stop();
            logger.LogInformation(
                "HTTP {Method} {Url} → {StatusCode} in {ElapsedMs}ms",
                request.Method, request.RequestUri, (int)response.StatusCode, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "HTTP {Method} {Url} failed after {ElapsedMs}ms",
                request.Method, request.RequestUri, sw.ElapsedMilliseconds);
            throw;
        }
    }
}