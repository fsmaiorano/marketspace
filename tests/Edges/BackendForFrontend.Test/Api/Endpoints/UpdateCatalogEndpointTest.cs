using BackendForFrontend.Api.Catalog.Dtos;
using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.UpdateCatalog;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using Catalog.Test.Api;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;

namespace BackendForFrontend.Test.Api.Endpoints;

public class UpdateCatalogEndpointTest(BackendForFrontendFactory factory) : HttpFixture(factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Updated_Successfully()
    {
        CatalogApiFactory factory = new();
        using IServiceScope scope = factory.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();

        catalog.Id = CatalogId.Of(Guid.NewGuid());

        dbContext.Catalogs.Add(catalog);
        await dbContext.SaveChangesAsync();

        UpdateCatalogCommand command = new()
        {
            Id = catalog.Id.Value,
            Name = "Updated Catalog Name",
            Description = "Updated Catalog Description",
            ImageUrl = "http://example.com/updated-image.jpg",
            Categories = catalog.Categories,
            Price = 49.99m,
            MerchantId = Guid.NewGuid()
        };

        HttpResponseMessage response = await _client.PutAsJsonAsync($"/api/catalog/{command.Id}", command);
        response.EnsureSuccessStatusCode();

        Result<UpdateCatalogResponse>? result =
            await response.Content.ReadFromJsonAsync<Result<UpdateCatalogResponse>>();
        result?.IsSuccess.Should().BeTrue();
    }
}