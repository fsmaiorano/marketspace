using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Services.CurrentUser;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => Principal?.FindFirstValue(ClaimTypes.Name);

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;
}
