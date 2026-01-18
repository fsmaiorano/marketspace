using Builder;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Services;

public class TokenServiceUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task CreateTokensAsync_ShouldGenerateValidTokens()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var ipAddress = Faker.Internet.Ip();

        var result = await TokenService.CreateTokensAsync(user, ipAddress);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.True(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.True(result.RefreshTokenExpiration > DateTime.UtcNow);

        var storedToken = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);
        
        Assert.NotNull(storedToken);
        Assert.Equal(user.Id, storedToken.UserId);
        Assert.Equal(ipAddress, storedToken.CreatedByIp);
    }

    [Fact]
    public async Task RefreshAsync_WithValidTokens_ShouldReturnNewTokenPair()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var tokens = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        var ipAddress = Faker.Internet.Ip();

        var result = await TokenService.RefreshAsync(tokens.AccessToken, tokens.RefreshToken, ipAddress);

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEqual(tokens.AccessToken, result.AccessToken);
        Assert.NotEqual(tokens.RefreshToken, result.RefreshToken);

        var oldToken = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);
        Assert.NotNull(oldToken);
        Assert.NotNull(oldToken.Revoked);
        Assert.Equal(ipAddress, oldToken.RevokedByIp);

        var newToken = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == result.RefreshToken);
        Assert.NotNull(newToken);
        Assert.True(newToken.IsActive);
    }

    [Fact]
    public async Task RefreshAsync_WithInvalidRefreshToken_ShouldReturnNull()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var tokens = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        var invalidRefreshToken = "invalid-refresh-token";

        var result = await TokenService.RefreshAsync(tokens.AccessToken, invalidRefreshToken, Faker.Internet.Ip());

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_WithRevokedRefreshToken_ShouldReturnNull()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var tokens = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        
        await TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken, Faker.Internet.Ip());

        var result = await TokenService.RefreshAsync(tokens.AccessToken, tokens.RefreshToken, Faker.Internet.Ip());
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredRefreshToken_ShouldReturnNull()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var expiredToken = UserBuilder.CreateRefreshToken(user.Id, expires: DateTime.UtcNow.AddDays(-1));
        Context.RefreshTokens.Add(expiredToken);
        await Context.SaveChangesAsync();

        var tokens = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        var result = await TokenService.RefreshAsync(tokens.AccessToken, expiredToken.Token, Faker.Internet.Ip());

        Assert.Null(result);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithValidToken_ShouldRevokeToken()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var tokens = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        var ipAddress = Faker.Internet.Ip();

        await TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken, ipAddress);

        var revokedToken = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);
        Assert.NotNull(revokedToken);
        Assert.NotNull(revokedToken.Revoked);
        Assert.Equal(ipAddress, revokedToken.RevokedByIp);
        Assert.False(revokedToken.IsActive);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithInvalidToken_ShouldNotThrowException()
    {
        const string invalidToken = "invalid-token";
        var ipAddress = Faker.Internet.Ip();
        await TokenService.RevokeRefreshTokenAsync(invalidToken, ipAddress);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WithAlreadyRevokedToken_ShouldNotUpdateToken()
    {
        var user = await CreateTestUserAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var tokens = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        await TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken, Faker.Internet.Ip());

        var firstRevokedState = await Context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);

        await TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken, Faker.Internet.Ip());

        var secondRevokedState = await Context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);
        Assert.Equal(firstRevokedState!.Revoked, secondRevokedState!.Revoked);
        Assert.Equal(firstRevokedState.RevokedByIp, secondRevokedState.RevokedByIp);
    }
}

