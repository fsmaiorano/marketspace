using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Services.Correlation;

public class CorrelationIdService(IHttpContextAccessor httpContextAccessor) : ICorrelationIdService
{
    public string GetCorrelationId()
    {
        return httpContextAccessor.HttpContext?.Items["X-Correlation-ID"]?.ToString() 
               ?? Guid.NewGuid().ToString();
    }
}