using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.GetCatalog;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace Catalog.Test.Api.Endpoints;

public class GetCatalogEndpointTest(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IGetCatalogByIdHandler> _mockHandler = new();
    private readonly CatalogApiFactory _factory = factory;

    [Fact]
    public async Task Can_Get_Catalog_By_Id_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        const int createCatalogCount = 5;
        for (int i = 0; i < createCatalogCount; i++)
        {
            CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();
            catalog.Id = CatalogId.Of(Guid.NewGuid());
            dbContext.Catalogs.Add(catalog);
        }

        await dbContext.SaveChangesAsync();

        HttpRequestMessage request = new(HttpMethod.Get, $"/catalog?PageIndex=1&PageSize=10");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Result<GetCatalogResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetCatalogResult>>();
        responseResult?.Data.Should().NotBeNull();
    }
}