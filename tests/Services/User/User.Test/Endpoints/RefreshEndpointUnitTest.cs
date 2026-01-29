using Builder;
using System.Net;
using System.Net.Http.Json;
using User.Api.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class RefreshEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task Refresh_WithValidTokens_ShouldReturn200AndNewTokens()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        HttpResponseMessage response = await DoPost("/api/auth/refresh", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AuthResponse? result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.NotEqual(tokens.AccessToken, result.AccessToken);
        Assert.NotEqual(tokens.RefreshToken, result.RefreshToken);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturn400()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: "invalid-refresh-token");

        HttpResponseMessage response = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithRevokedToken_ShouldReturn400()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));

        await TokenService.RevokeRefreshTokenAsync(tokens.RefreshToken, Faker.Internet.Ip());

        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        HttpResponseMessage response = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_AfterUsingRefreshToken_OldTokenShouldBeInvalid()
    {
        AuthResponse tokens = await CreateTestUserWithTokensAsync(Faker.Internet.Email(), Faker.Internet.Password(25, false, string.Empty, "1"));
        RefreshRequest request = UserBuilder.CreateRefreshRequest(
            accessToken: tokens.AccessToken,
            refreshToken: tokens.RefreshToken);

        HttpResponseMessage response1 = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

        HttpResponseMessage response2 = await DoPost("/api/auth/refresh", request);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
}