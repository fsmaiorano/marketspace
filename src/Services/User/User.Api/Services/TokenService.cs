namespace User.Api.Services;

public class JwtSettings
{
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

public interface ITokenService
{
    Task<AuthResponse> CreateTokensAsync(ApplicationUser user, string ipAddress);
    Task<AuthResponse?> RefreshAsync(string accessToken, string refreshToken, string ipAddress);
    Task RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
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
        (string Token, DateTime Expires) accessToken = await GenerateAccessToken(user);
        (string Token, DateTime Expires, DateTime Created, string CreatedByIp) refreshToken = GenerateRefreshToken(ipAddress);

        RefreshToken refreshEntity = new RefreshToken
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
        ClaimsPrincipal? principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal == null) return null;

        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return null;

        RefreshToken? stored = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
        if (stored == null || !stored.IsActive) return null;

        // revoke old token
        stored.Revoked = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;

        // create new pair
        ApplicationUser? user = await userManager.FindByIdAsync(userId);
        if (user == null) return null;

        (string Token, DateTime Expires) newAccess = await GenerateAccessToken(user);
        (string Token, DateTime Expires, DateTime Created, string CreatedByIp) newRefresh = GenerateRefreshToken(ipAddress);

        RefreshToken refreshEntity = new RefreshToken
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
        RefreshToken? stored = await db.RefreshTokens.SingleOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored == null || !stored.IsActive) return;

        stored.Revoked = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        await db.SaveChangesAsync();
    }

    private async Task<(string Token, DateTime Expires)> GenerateAccessToken(ApplicationUser user)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];
        
        if (!string.IsNullOrWhiteSpace(user.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
        
        if (!string.IsNullOrWhiteSpace(user.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

        IList<string> roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        DateTime expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        string? tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }

    private (string Token, DateTime Expires, DateTime Created, string CreatedByIp) GenerateRefreshToken(string ipAddress)
    {
        byte[] randomBytes = new byte[64];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        string token = Convert.ToBase64String(randomBytes);
        DateTime created = DateTime.UtcNow;
        DateTime expires = created.AddDays(_jwtSettings.RefreshTokenExpirationDays);
        return (token, expires, created, ipAddress);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateLifetime = false // we want to get principal from expired token
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken? securityToken);
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