using Builder;
using System.Net;
using System.Net.Http.Json;
using User.Api.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class LoginEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200AndTokens()
    {
        string? email = Faker.Internet.Email();
        string? password = Faker.Internet.Password(25, false, string.Empty, "1");
        await CreateTestUserAsync(email, password);

        AuthRequest request = UserBuilder.CreateLoginRequest(email: email, password: password);
        HttpResponseMessage response = await DoPost("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        AuthResponse? result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.True(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.True(result.RefreshTokenExpiration > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturn401()
    {
        AuthRequest request = UserBuilder.CreateLoginRequest(email: "nonexistent@example.com", password: "Password123!");
        HttpResponseMessage response = await DoPost("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        string? email = Faker.Internet.Email();
        string? password = Faker.Internet.Password(25);
        await CreateTestUserAsync(email, password);

        AuthRequest request = UserBuilder.CreateLoginRequest(email: email, password: "WrongPassword123!");
        HttpResponseMessage response = await DoPost("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_MultipleTimes_ShouldGenerateDifferentTokens()
    {
        string? email = Faker.Internet.Email();
        string? password = Faker.Internet.Password(25);
        await CreateTestUserAsync(email, password);

        AuthRequest request = UserBuilder.CreateLoginRequest(email: email, password: password);

        HttpResponseMessage response1 = await DoPost("/api/auth/login", request);
        AuthResponse? result1 = await response1.Content.ReadFromJsonAsync<AuthResponse>();

        HttpResponseMessage response2 = await DoPost("/api/auth/login", request);
        AuthResponse? result2 = await response2.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.AccessToken, result2.AccessToken);
        Assert.NotEqual(result1.RefreshToken, result2.RefreshToken);
    }
}

