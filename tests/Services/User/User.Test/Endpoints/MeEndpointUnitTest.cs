using System.Net;
using System.Net.Http.Json;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class MeEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Me_WithValidToken_ShouldReturn200AndUserInfo()
    {
        var email = Faker.Internet.Email();
        var tokens = await CreateTestUserWithTokensAsync(email, Faker.Internet.Password(25, false, string.Empty, "1"));

        var response = await DoGet("/api/auth/me", token: tokens.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("userId"));
        Assert.True(result.ContainsKey("email"));
        Assert.Equal(email, result["email"]);
    }

    [Fact]
    public async Task Me_WithoutToken_ShouldReturn401()
    {
        var response = await DoGet("/api/auth/me", null!);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithInvalidToken_ShouldReturn401()
    {
        var response = await DoGet("/api/auth/me", token: "invalid-token");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

