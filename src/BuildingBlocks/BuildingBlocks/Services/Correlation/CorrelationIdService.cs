using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Services.Correlation;

public class CorrelationIdService(IHttpContextAccessor httpContextAccessor) : ICorrelationIdService
{
    private string? _correlationId;

    public string GetCorrelationId()
    {
        if (httpContextAccessor.HttpContext?.Items.TryGetValue("CorrelationId", out var contextCorrelationId) == true)
            return contextCorrelationId?.ToString() ?? GenerateNewCorrelationId();

        return _correlationId ?? GenerateNewCorrelationId();
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationId = correlationId;

        if (httpContextAccessor.HttpContext != null)
            httpContextAccessor.HttpContext.Items["CorrelationId"] = correlationId;
    }

    private static string GenerateNewCorrelationId() => Guid.CreateVersion7().ToString();
}