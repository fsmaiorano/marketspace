using Bogus;
using Microsoft.Extensions.DependencyInjection;
using User.Api.Data;
using User.Api.Models;
using User.Api.Services;
using User.Test.Fixtures;

namespace User.Test.Base;

public abstract class BaseTest : IClassFixture<TestFixture>, IDisposable
{
    #region Fields and Properties

    private readonly IServiceScope _scope;

    private TestFixture Fixture { get; }

    protected UserDbContext Context => _scope.ServiceProvider.GetRequiredService<UserDbContext>();

    protected UserManager<ApplicationUser> UserManager =>
        _scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // protected IDomainEventDispatcher DomainEventDispatcher =>
    //     _scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

    protected ITokenService TokenService => _scope.ServiceProvider.GetRequiredService<ITokenService>();
    protected readonly Faker Faker;

    protected HttpClient HttpClient { get; }

    #endregion

    protected BaseTest(TestFixture fixture)
    {
        Fixture = fixture;
        _scope = fixture.Services.CreateScope();
        HttpClient = fixture.CreateClient();
        Faker = new Faker();

        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }

    #region HTTP Request Helpers

    protected async Task<HttpResponseMessage> DoPost(string method, object request, string token = "",
        string culture = "en-US")
        => await Fixture.DoPost(method, request, token, culture);

    protected async Task<HttpResponseMessage> DoGet(string method, string token = "", string culture = "en-US")
        => await Fixture.DoGet(method, token, culture);

    protected async Task<HttpResponseMessage> DoPut(string method, object request, string token = "",
        string culture = "en-US")
        => await Fixture.DoPut(method, request, token, culture);

    protected async Task<HttpResponseMessage> DoPatch(string method, object request, string token = "",
        string culture = "en-US")
        => await Fixture.DoPatch(method, request, token, culture);

    protected async Task<HttpResponseMessage> DoDelete(string method, string token = "", string culture = "en-US")
        => await Fixture.DoDelete(method, token, culture);

    #endregion

    #region Lifecycle

    public void Dispose()
    {
        _scope?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    protected async Task<ApplicationUser> CreateTestUserAsync(string email, string password)
    {
        ApplicationUser user = new() { UserName = email, Email = email };
        IdentityResult result = await UserManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception(
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        return user;
    }

    private async Task<ApplicationUser> CreateTestUserAsync()
    {
        ApplicationUser user = new() { UserName = Faker.Internet.UserName(), Email = Faker.Internet.Email() };
        IdentityResult result = await UserManager.CreateAsync(user, Faker.Internet.Password(25, false, string.Empty, "1"));
        if (!result.Succeeded)
            throw new Exception(
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        return user;
    }

    protected async Task<AuthResponse> CreateTestUserWithTokensAsync(string email, string password)
    {
        ApplicationUser user = await CreateTestUserAsync(email, password);
        return await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
    }

    protected async Task<AuthResponse> CreateTestUserWithTokensAsync()
    {
        ApplicationUser user = await CreateTestUserAsync();
        return await TokenService.CreateTokensAsync(user, Faker.Internet.Ip());
    }
}