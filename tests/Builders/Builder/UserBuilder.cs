using Bogus;
using User.Data.Models;
using User.Models;

namespace Builder;

public class UserBuilder
{
    private static readonly Faker _faker = new();

    public static Faker<RegisterRequest> CreateRegisterRequest()
    {
        return new Faker<RegisterRequest>()
            .CustomInstantiator(f => new RegisterRequest
            {
                Email = f.Internet.Email(), 
                Password = f.Internet.Password(8, false, "", "@1Aa")
            });
    }

    public static Faker<AuthRequest> CreateLoginRequest()
    {
        return new Faker<AuthRequest>()
            .CustomInstantiator(f => new AuthRequest
            {
                Email = f.Internet.Email(), 
                Password = f.Internet.Password(8, false, "", "@1Aa")
            });
    }

    public static Faker<RefreshRequest> CreateRefreshRequest(string? accessToken = null, string? refreshToken = null)
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
        return new ApplicationUser 
        { 
            Email = userEmail, 
            UserName = userName ?? userEmail, 
            EmailConfirmed = true 
        };
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
