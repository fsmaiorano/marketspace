using BuildingBlocks.Services.Correlation;

namespace BuildingBlocks.Http;

public class CorrelationIdHandler(ICorrelationIdService correlationIdService) : DelegatingHandler
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string correlationId = correlationIdService.GetCorrelationId();

        if (!request.Headers.Contains(CorrelationIdHeader))
            request.Headers.Add(CorrelationIdHeader, correlationId);

        return base.SendAsync(request, cancellationToken);
    }
}
