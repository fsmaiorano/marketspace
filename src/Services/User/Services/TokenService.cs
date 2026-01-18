using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using User.Data;
using User.Data.Models;
using User.Models;

namespace User.Services;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

public class TokenService(
    IOptions<JwtSettings> jwtOptions,
    UserManager<ApplicationUser> userManager,
    UserDbContext db)
    : ITokenService
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<AuthResponse> CreateTokensAsync(ApplicationUser user, string ipAddress)
    {
        var accessToken = await GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(ipAddress);

        var refreshEntity = new RefreshToken
        {
            Token = refreshToken.Token,
            Expires = refreshToken.Expires,
            Created = refreshToken.Created,
            CreatedByIp = refreshToken.CreatedByIp,
            UserId = user.Id
        };

        db.RefreshTokens.Add(refreshEntity);
        await db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiration = accessToken.Expires,
            RefreshToken = refreshEntity.Token,
            RefreshTokenExpiration = refreshEntity.Expires
        };
    }

    public async Task<AuthResponse?> RefreshAsync(string accessToken, string refreshToken, string ipAddress)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return null;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        var stored = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
        if (stored == null || !stored.IsActive) return null;

        // revoke old token
        stored.Revoked = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;

        // create new pair
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var newAccess = await GenerateAccessToken(user);
        var newRefresh = GenerateRefreshToken(ipAddress);

        var refreshEntity = new RefreshToken
        {
            Token = newRefresh.Token,
            Expires = newRefresh.Expires,
            Created = newRefresh.Created,
            CreatedByIp = newRefresh.CreatedByIp,
            UserId = user.Id
        };

        db.RefreshTokens.Add(refreshEntity);
        await db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = newAccess.Token,
            AccessTokenExpiration = newAccess.Expires,
            RefreshToken = refreshEntity.Token,
            RefreshTokenExpiration = refreshEntity.Expires
        };
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
    {
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored == null || !stored.IsActive) return;

        stored.Revoked = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        await db.SaveChangesAsync();
    }

    private async Task<(string Token, DateTime Expires)> GenerateAccessToken(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }

    private (string Token, DateTime Expires, DateTime Created, string CreatedByIp) GenerateRefreshToken(string ipAddress)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);
        var created = DateTime.UtcNow;
        var expires = created.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        return (token, expires, created, ipAddress);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateLifetime = false // we want to get principal from expired token
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                return null;
            return principal;
        }
        catch
        {
            return null;
        }
    }
}