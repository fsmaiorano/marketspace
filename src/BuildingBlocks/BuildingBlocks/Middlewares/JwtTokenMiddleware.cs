using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Serilog;

namespace BuildingBlocks.Middlewares;

public class JwtTokenMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string? token = context.Request.Headers.Authorization.FirstOrDefault()?.Replace("Bearer ", "");
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                JwtSecurityTokenHandler handler = new();
                JwtSecurityToken jwtToken = handler.ReadJwtToken(token);
                foreach (Claim claim in jwtToken.Claims)
                {
                    Log.Information($"JWT Claim: {claim.Type} = {claim.Value}");
                }
                // Optionally: add custom logic here
            }
            catch (Exception ex)
            {
                Log.Warning($"JWT token parsing failed: {ex.Message}");
            }
        }
        await next(context);
    }
}

