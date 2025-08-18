namespace Catalog.Test.Api;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

public class MinioFixture
{
    private readonly IContainer _container;
    public string Endpoint { get; private set; }
    public string AccessKey => "admin";
    public string SecretKey => "admin123";

    public MinioFixture()
    {
        _container = new ContainerBuilder()
            .WithImage("minio/minio:latest")
            .WithEnvironment("MINIO_ROOT_USER", AccessKey)
            .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
            .WithPortBinding(9000, true)
            .WithCommand("server", "/data")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var port = _container.GetMappedPublicPort(9000);
        Endpoint = $"http://localhost:{port}";
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}
