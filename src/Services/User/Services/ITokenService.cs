using User.Data.Models;
using User.DTOs;

namespace User.Services;

public interface ITokenService
{
    Task<AuthResponse> CreateTokensAsync(ApplicationUser user, string ipAddress);
    Task<AuthResponse?> RefreshAsync(string accessToken, string refreshToken, string ipAddress);
    Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
}