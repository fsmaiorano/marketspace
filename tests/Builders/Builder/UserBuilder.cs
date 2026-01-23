using Bogus;
using User.Api.Data.Models;
using User.Api.Models;

namespace Builder;

public class UserBuilder
{
    private static readonly Faker _faker = new();

    public static RegisterRequest CreateRegisterRequest(string? email = null, string? password = null)
    {
        return new Faker<RegisterRequest>()
            .CustomInstantiator(f => new RegisterRequest
            {
                Email = email ?? f.Internet.Email(),
                Password = password ?? f.Internet.Password(8, false, "", "@1Aa")
            });
    }

    public static AuthRequest CreateLoginRequest(string? email, string? password)
    {
        return new Faker<AuthRequest>()
            .CustomInstantiator(f => new AuthRequest
            {
                Email = email ?? f.Internet.Email(),
                Password = password ?? f.Internet.Password(8, false, "", "@1Aa")
            });
    }

    public static RefreshRequest CreateRefreshRequest(string? accessToken = null, string? refreshToken = null)
    {
        return new Faker<RefreshRequest>()
            .CustomInstantiator(f => new RefreshRequest
            {
                AccessToken = accessToken ?? f.Random.AlphaNumeric(100),
                RefreshToken = refreshToken ?? f.Random.AlphaNumeric(100)
            });
    }

    public static ApplicationUser CreateApplicationUser(
        string? email = null,
        string? userName = null)
    {
        var userEmail = email ?? _faker.Internet.Email();
        return new ApplicationUser { Email = userEmail, UserName = userName ?? userEmail, EmailConfirmed = true };
    }

    public static RefreshToken CreateRefreshToken(
        string userId,
        string? token = null,
        DateTime? expires = null,
        bool isRevoked = false)
    {
        var refreshToken = new RefreshToken
        {
            Token = token ?? Convert.ToBase64String(_faker.Random.Bytes(64)),
            UserId = userId,
            Expires = expires ?? DateTime.UtcNow.AddDays(30),
            Created = DateTime.UtcNow,
            CreatedByIp = _faker.Internet.Ip()
        };

        if (!isRevoked) return refreshToken;
        refreshToken.Revoked = DateTime.UtcNow;
        refreshToken.RevokedByIp = _faker.Internet.Ip();

        return refreshToken;
    }
}