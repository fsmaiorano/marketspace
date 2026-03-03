using System.Net;
using System.Net.Http.Json;
using User.Api.Models;
using User.Test.Fixtures;

namespace User.Test.Endpoints;

public class UpdateUserTypeEndpointUnitTest(TestFixture fixture) : Base.BaseTest(fixture)
{
    [Fact]
    public async Task UpdateUserType_WithValidData_ShouldReturn200()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = Faker.Internet.Password(10, false, string.Empty, "1Aa@");
        AuthResponse authResponse = await CreateTestUserWithTokensAsync(email, password);
        
        ApplicationUser? user = await UserManager.FindByEmailAsync(email);
        Assert.NotNull(user);
        
        UpdateUserTypeRequest request = new()
        {
            UserId = user.Id,
            UserType = UserTypeEnum.Merchant
        };

        // Act
        HttpResponseMessage response = await DoPut("/api/auth/update-user-type", request, authResponse.AccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(result);

        // Verify the user type was actually updated - reload from database
        Context.ChangeTracker.Clear();
        ApplicationUser? updatedUser = await UserManager.FindByIdAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(UserTypeEnum.Merchant, updatedUser.UserType);
    }

    [Fact]
    public async Task UpdateUserType_WithInvalidUserId_ShouldReturn404()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = Faker.Internet.Password(10, false, string.Empty, "1Aa@");
        AuthResponse authResponse = await CreateTestUserWithTokensAsync(email, password);
        
        UpdateUserTypeRequest request = new()
        {
            UserId = "non-existent-user-id",
            UserType = UserTypeEnum.Merchant
        };

        // Act
        HttpResponseMessage response = await DoPut("/api/auth/update-user-type", request, authResponse.AccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserType_WithoutAuthentication_ShouldReturn401()
    {
        // Arrange
        UpdateUserTypeRequest request = new()
        {
            UserId = "some-user-id",
            UserType = UserTypeEnum.Merchant
        };

        // Act
        HttpResponseMessage response = await DoPut("/api/auth/update-user-type", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUserType_ChangeFromCustomerToMerchant_ShouldReturn200()
    {
        // Arrange
        string email = Faker.Internet.Email();
        string password = Faker.Internet.Password(10, false, string.Empty, "1Aa@");
        ApplicationUser user = await CreateTestUserAsync(email, password);
        user.UserType = UserTypeEnum.Customer;
        await UserManager.UpdateAsync(user);
        
        AuthResponse authResponse = await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
        
        UpdateUserTypeRequest request = new()
        {
            UserId = user.Id,
            UserType = UserTypeEnum.Merchant
        };

        // Act
        HttpResponseMessage response = await DoPut("/api/auth/update-user-type", request, authResponse.AccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        // Verify the user type was changed - reload from database
        Context.ChangeTracker.Clear();
        ApplicationUser? updatedUser = await UserManager.FindByIdAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(UserTypeEnum.Merchant, updatedUser.UserType);
    }
}





