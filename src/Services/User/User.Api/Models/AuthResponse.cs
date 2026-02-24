namespace User.Api.Models;

public class AuthResponse
{
    public string AccessToken { get; init; } = null!;
    public DateTime AccessTokenExpiration { get; init; }
    public string RefreshToken { get; init; } = null!;
    public DateTime RefreshTokenExpiration { get; init; }
}