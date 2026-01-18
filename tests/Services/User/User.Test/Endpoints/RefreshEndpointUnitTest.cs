using Builder;
using System.Net;
using System.Net.Http.Json;
using User.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class RefreshEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Refresh_WithValidTokens_ShouldReturn200AndNewTokens()
    {
        var tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        var response = await DoPost("/api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEqual(tokens.AccessToken, result.AccessToken);
        Assert.NotEqual(tokens.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturn400()
    {
        var tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: "invalid-refresh-token");

        var response = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldReturn400()
    {
        var tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));

        await TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken, Faker.Internet.Ip());

        var request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        var response = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterUsingRefreshToken_OldTokenShouldBeInvalid()
    {
        var tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        var request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        var response1 = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        var response2 = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
}