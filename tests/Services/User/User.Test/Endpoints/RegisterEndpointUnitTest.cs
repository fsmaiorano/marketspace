using Builder;
using System.Net;
using System.Net.Http.Json;
using User.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class RegisterEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndTokens()
    {
        var request = UserBuilder.CreateRegisterRequest();
        var response = await DoPost("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.True(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.True(result.RefreshTokenExpiration > DateTime.UtcNow);

        var user = await UserManager.FindByEmailAsync(request.Email);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturn400()
    {
        var existingEmail = Faker.Internet.Email();
        await CreateTestUserAsync(existingEmail, Faker.Internet.Password(25, false, string.Empty, "1"));
        
        var request = UserBuilder.CreateRegisterRequest(email: existingEmail);
        var response = await DoPost("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        var request = UserBuilder.CreateRegisterRequest(password: "123");
        var response = await DoPost("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        var request = UserBuilder.CreateRegisterRequest(email: "invalid-email");
        var response = await DoPost("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

