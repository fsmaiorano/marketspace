using Builder;
using System.Net;
using System.Net.Http.Json;
using User.Data.Models;
using User.Models;
using User.Test.Base;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class RegisterEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Register_WithValidData_ShouldReturn200AndTokens()
    {
        RegisterRequest request = UserBuilder.CreateRegisterRequest();
        HttpResponseMessage response = await DoPost("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        AuthResponse? result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.NotEmpty(result.RefreshToken);
        Assert.True(result.AccessTokenExpiration > DateTime.UtcNow);
        Assert.True(result.RefreshTokenExpiration > DateTime.UtcNow);

        ApplicationUser? user = await UserManager.FindByEmailAsync(request.Email);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturn400()
    {
        string? existingEmail = Faker.Internet.Email();
        await CreateTestUserAsync(existingEmail, Faker.Internet.Password(25, false, string.Empty, "1"));
        
        RegisterRequest request = UserBuilder.CreateRegisterRequest(email: existingEmail);
        HttpResponseMessage response = await DoPost("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        RegisterRequest request = UserBuilder.CreateRegisterRequest(password: "123");
        HttpResponseMessage response = await DoPost("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        RegisterRequest request = UserBuilder.CreateRegisterRequest(email: "invalid-email");
        HttpResponseMessage response = await DoPost("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

