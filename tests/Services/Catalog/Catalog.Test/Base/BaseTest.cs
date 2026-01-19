using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Catalog.Api.Infrastructure.Data;
using Catalog.Test.Fixtures;
using BuildingBlocks.Storage.Minio;

namespace Catalog.Test.Base;

public abstract class BaseTest : IClassFixture<TestFixture>, IDisposable
{
    #region Fields and Properties

    private readonly IServiceScope _scope;

    private TestFixture Fixture { get; }

    protected CatalogDbContext Context => _scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

    protected ICatalogDbContext DbContext => _scope.ServiceProvider.GetRequiredService<ICatalogDbContext>();

    protected IMinioBucket MinioBucket => _scope.ServiceProvider.GetRequiredService<IMinioBucket>();

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
}
