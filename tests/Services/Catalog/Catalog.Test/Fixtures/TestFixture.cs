using BaseTest.Fixtures;
using BuildingBlocks.Storage.Minio;
using Catalog.Test.Mocks;
using Microsoft.AspNetCore.TestHost;

namespace Catalog.Test.Fixtures;

public sealed class TestFixture :  BaseTestFixture<CatalogProgram>
{

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Garantir configuração padrão do BaseTestFixture (registro do InMemoryDb etc)
        base.ConfigureWebHost(builder);

        // Registrar IMinioBucket para testes (garante presença mesmo que a implementação real tenha sido removida)
        builder.ConfigureTestServices(services =>
        {
            services.AddScoped<IMinioBucket, MockMinioBucket>();
        });

        // ...existing code...
    }
}
