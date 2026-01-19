using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.GetCatalog;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Test.Base;
using Catalog.Test.Fixtures;
using FluentAssertions;
using System.Net.Http.Json;

namespace Catalog.Test.Endpoints;

public class GetCatalogEndpointUnitTest(TestFixture fixture) : BaseTest(fixture)
{
    [Fact]
    public async Task Can_Get_Catalog_By_Id_Endpoint()
    {
        const int createCatalogCount = 5;
        for (int i = 0; i < createCatalogCount; i++)
        {
            CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();
            catalog.Id = CatalogId.Of(Guid.CreateVersion7());
            Context.Catalogs.Add(catalog);
        }

        await Context.SaveChangesAsync();

        HttpResponseMessage response = await DoGet("/catalog?PageIndex=1&PageSize=10");
        response.EnsureSuccessStatusCode();
        Result<GetCatalogResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetCatalogResult>>();
        responseResult?.Data.Should().NotBeNull();
    }
}
