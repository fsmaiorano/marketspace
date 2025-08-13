using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Http;

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Sending request to {Url}", request.RequestUri);

            var response = await base.SendAsync(request, cancellationToken);
            logger.LogInformation("Received response with status code {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Request to {Url} failed with status code {StatusCode}", request.RequestUri,
                    response.StatusCode);
            }

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while sending the request.");
            throw;
        }
    }
}