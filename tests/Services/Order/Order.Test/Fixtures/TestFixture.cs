using Order.Api;
using Order.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Hosting;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Order.Test.Fixtures;

public sealed class TestFixture : WebApplicationFactory<OrderProgram>, IAsyncLifetime
{
    private HttpClient? _httpClient;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName != null &&
                            (d.ServiceType == typeof(DbContextOptions<OrderDbContext>) ||
                             d.ServiceType == typeof(OrderDbContext) ||
                             d.ServiceType == typeof(IOrderDbContext) ||
                             d.ServiceType.FullName.Contains(nameof(OrderDbContext)) ||
                             d.ServiceType.FullName.Contains(nameof(IOrderDbContext)) ||
                             d.ServiceType.FullName.Contains("EntityFramework") ||
                             d.ServiceType.FullName.Contains("Npgsql")))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            services.AddDbContext<OrderDbContext>(options =>
                options.UseInMemoryDatabase("InMemoryDbForTesting"));

            services.AddScoped<IOrderDbContext, OrderDbContext>();

            // Remove Serilog services
            services.RemoveAll<ILoggerFactory>();
            services.RemoveAll(typeof(ILogger<>));
            services.RemoveAll<Serilog.ILogger>();
            services.RemoveAll<DiagnosticContext>();
            
            // Add simple console logging for tests
            services.AddLogging(loggingBuilder => 
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.SetMinimumLevel(LogLevel.Warning);
            });
        });

        builder.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

        base.ConfigureWebHost(builder);
    }

    private HttpClient GetHttpClient()
    {
        return _httpClient ??= CreateClient();
    }

    #region HTTP Helper Methods

    public async Task<HttpResponseMessage> DoPost(string method, object request, string token = "",
        string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PostAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoGet(string method, string token = "", string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.GetAsync(method);
    }

    public async Task<HttpResponseMessage> DoPut(string method, object request, string token = "",
        string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PutAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoPatch(string method, object request, string token = "",
        string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
        ChangeRequestCulture(culture, client);
        AuthorizeRequest(token, client);
        return await client.PatchAsJsonAsync(method, request);
    }

    public async Task<HttpResponseMessage> DoDelete(string method, string token = "", string culture = "en-US")
    {
        HttpClient client = GetHttpClient();
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
