using System.Net;
using System.Net.Http.Json;
using User.Api.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class MeEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Me_WithValidToken_ShouldReturn200AndUserInfo()
    {
        string? email = Faker.Internet.Email();
        AuthResponse tokens = await CreateTestUserWithTokensAsync(email, Faker.Internet.Password(25, false, string.Empty, "1"));

        HttpResponseMessage response = await DoGet("/api/auth/me", token: tokens.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        Dictionary<string, string>? result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("userId"));
        Assert.True(result.ContainsKey("email"));
        Assert.Equal(email, result["email"]);
    }

    [Fact]
    public async Task Me_WithoutToken_ShouldReturn401()
    {
        HttpResponseMessage response = await DoGet("/api/auth/me", null!);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithInvalidToken_ShouldReturn401()
    {
        HttpResponseMessage response = await DoGet("/api/auth/me", token: "invalid-token");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

