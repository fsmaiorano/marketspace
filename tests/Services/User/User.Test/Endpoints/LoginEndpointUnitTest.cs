using Builder;
using System.Net;
using System.Net.Http.Json;
using User.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class LoginEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturn200AndTokens()
    {
        var email = Faker.Internet.Email();
        var password = Faker.Internet.Password(25, false, string.Empty, "1");
        await CreateTestUserAsync(email, password);

        var request = UserBuilder.CreateLoginRequest(email: email, password: password);
        var response = await DoPost("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.True(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.True(result.RefreshTokenExpiration > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ShouldReturn401()
    {
        var request = UserBuilder.CreateLoginRequest(email: "nonexistent@example.com", password: "Password123!");
        var response = await DoPost("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        var email = Faker.Internet.Email();
        var password = Faker.Internet.Password(25);
        await CreateTestUserAsync(email, password);

        var request = UserBuilder.CreateLoginRequest(email: email, password: "WrongPassword123!");
        var response = await DoPost("/api/auth/login", request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_MultipleTimes_ShouldGenerateDifferentTokens()
    {
        var email = Faker.Internet.Email();
        var password = Faker.Internet.Password(25);
        await CreateTestUserAsync(email, password);

        var request = UserBuilder.CreateLoginRequest(email: email, password: password);

        var response1 = await DoPost("/api/auth/login", request);
        var result1 = await response1.Content.ReadFromJsonAsync<AuthResponse>();

        var response2 = await DoPost("/api/auth/login", request);
        var result2 = await response2.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.AccessToken, result2.AccessToken);
        Assert.NotEqual(result1.RefreshToken, result2.RefreshToken);
    }
}

