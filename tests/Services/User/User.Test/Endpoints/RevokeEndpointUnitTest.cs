using Builder;
using System.Net;
using User.Data.Models;
using User.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class RevokeEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Revoke_WithValidRefreshToken_ShouldReturn204()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(),
            Faker.Internet.Password(25, false, string.Empty, "1"));
        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        HttpResponseMessage response = await DoPost("/api/auth/revoke", request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        RefreshToken? revokedToken = await Context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == tokens.RefreshToken);

        Assert.NotNull(revokedToken);
        Assert.NotNull(revokedToken.Revoked);
        Assert.False(revokedToken.IsActive);
    }

    [Fact]
    public async Task Revoke_WithInvalidRefreshToken_ShouldReturn204()
    {
        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: "some-token",
            refreshToken: "invalid-refresh-token");

        HttpResponseMessage response = await DoPost("/api/auth/revoke", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Revoke_AlreadyRevokedToken_ShouldReturn204()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(),
            Faker.Internet.Password(25, false, string.Empty, "1"));
        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        await DoPost("/api/auth/revoke", request);

        HttpResponseMessage response = await DoPost("/api/auth/revoke", request);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Revoke_ThenRefresh_ShouldFail()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(),
            Faker.Internet.Password(25, false, string.Empty, "1"));
        RefreshRequest revokeRequest = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        HttpResponseMessage revokeResponse = await DoPost("/api/auth/revoke", revokeRequest);
        Assert.Equal(HttpStatusCode.NoContent, revokeResponse.StatusCode);

        HttpResponseMessage refreshResponse = await DoPost("/api/auth/refresh", revokeRequest);

        Assert.Equal(HttpStatusCode.BadRequest, refreshResponse.StatusCode);
    }
}