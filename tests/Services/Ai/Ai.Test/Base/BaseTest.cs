using Ai.Test.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace Ai.Test.Base;

public abstract class BaseTest : IClassFixture<TestFixture>, IDisposable
{
    private readonly IServiceScope _scope;

    protected HttpClient HttpClient { get; }

    protected BaseTest(TestFixture fixture)
    {
        _scope = fixture.Services.CreateScope();
        HttpClient = fixture.CreateClient();
    }

    protected async Task<HttpResponseMessage> DoPost(string endpoint, object request)
        => await HttpClient.PostAsJsonAsync(endpoint, request);

    protected async Task<HttpResponseMessage> DoGet(string endpoint)
        => await HttpClient.GetAsync(endpoint);

    public void Dispose()
    {
        _scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
