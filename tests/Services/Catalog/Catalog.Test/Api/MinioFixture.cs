namespace Catalog.Test.Api;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

public class MinioFixture
{
    public string MinioEndpoint { get; private set; } = string.Empty;
    private readonly IContainer _minioContainer;
    public string Endpoint { get; private set; }
    public string AccessKey => "admin";
    public string SecretKey => "admin123";

    public MinioFixture()
    {
        _minioContainer =  new ContainerBuilder()
            .WithImage("minio/minio:latest")
            .WithEnvironment("MINIO_ROOT_USER", AccessKey)
            .WithEnvironment("MINIO_ROOT_PASSWORD", SecretKey)
            .WithPortBinding(9000, true)
            .WithCommand("server", "/data", "--console-address", ":9001")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _minioContainer.StartAsync();
        ushort port = _minioContainer.GetMappedPublicPort(9000);
        MinioEndpoint = $"localhost:{port}";
    }

    public async Task DisposeAsync()
    {
        await _minioContainer.StopAsync();
        await _minioContainer.DisposeAsync();
    }
}
