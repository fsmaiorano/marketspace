using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using User.Api;
using User.Data;
using User.Data.Models;

namespace User.Test.Fixtures;

public sealed class TestFixture : WebApplicationFactory<UserProgram>, IAsyncLifetime
{
    private readonly string _databaseName = $"UserInMemoryTestDb_{Guid.NewGuid()}";
    private HttpClient? _httpClient;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UserDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<UserDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });

        base.ConfigureWebHost(builder);
    }

    public UserDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<UserDbContext>();
    }

    public UserManager<ApplicationUser> GetUserManager()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    }

    public ITokenService GetTokenService()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ITokenService>();
    }

    private HttpClient GetHttpClient()
    {
        return _httpClient ??= CreateClient();
    }

    #region HTTP Helper Methods

    public async Task<HttpResponseMessage> DoPost(string method, object request, string token = "",
        string culture = "en-US")
    {
        var client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PostAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoGet(string method, string token = "", string culture = "en-US")
    {
        var client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.GetAsync(method);
    }

    public async Task<HttpResponseMessage> DoPut(string method, object request, string token = "",
        string culture = "en-US")
    {
        var client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PutAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoPatch(string method, object request, string token = "",
        string culture = "en-US")
    {
        var client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PatchAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoDelete(string method, string token = "", string culture = "en-US")
    {
        var client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.DeleteAsync(method);
    }

    private static void AuthorizeRequest(string token, HttpClient client)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization = null;
            return;
        }

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static void ChangeRequestCulture(string culture, HttpClient client)
    {
        client.DefaultRequestHeaders.AcceptLanguage.Clear();
        client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(culture));
    }

    #endregion

    #region IAsyncLifetime

    public Task InitializeAsync() => Task.CompletedTask;

    public new async Task DisposeAsync()
    {
        if (_httpClient != null)
        {
            _httpClient.Dispose();
        }

        await base.DisposeAsync();
    }

    #endregion
}