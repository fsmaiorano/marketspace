using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.GetCatalogById;
using Catalog.Api.Domain.Entities;
using Catalog.Api.Domain.ValueObjects;
using Catalog.Api.Infrastructure.Data;
using FluentAssertions;
using Merchant.Api.Application.Merchant.GetMerchantById;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Json;

namespace Catalog.Test.Api.Endpoints;

public class GetCatalogByIdEndpointTest(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IGetCatalogByIdHandler> _mockHandler = new();
    private readonly CatalogApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Catalog_Exists()
    {
        Guid merchantId = Guid.NewGuid();
        GetCatalogByIdQuery query = new GetCatalogByIdQuery(merchantId);
        GetCatalogByIdResult result = new GetCatalogByIdResult
        {
            Id = Guid.NewGuid(),
            Name = "Test Catalog",
            Description = "This is a test catalog",
            ImageUrl = "http://example.com/image.jpg",
            Price = 99.99m,
            Categories = new List<string> { "Category1", "Category2" }
        };


        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetCatalogByIdQuery>()))
            .ReturnsAsync(Result<GetCatalogByIdResult>.Success(result));

        Result<GetCatalogByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_NotFound_When_Catalog_Does_Not_Exist()
    {
        Guid merchantId = Guid.NewGuid();
        GetCatalogByIdQuery query = new GetCatalogByIdQuery(merchantId);
        Result<GetCatalogByIdResult> result =
            Result<GetCatalogByIdResult>.Failure($"Catalog with ID {query.Id} not found.");

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetCatalogByIdQuery>()))
            .ReturnsAsync(result);

        Result<GetCatalogByIdResult> response = await _mockHandler.Object.HandleAsync(query);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be($"Catalog with ID {query.Id} not found.");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        GetCatalogByIdQuery query = new GetCatalogByIdQuery(Guid.NewGuid());
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetCatalogByIdQuery>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(query);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Get_Catalog_By_Id_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        CatalogEntity? catalog = CatalogBuilder.CreateCatalogFaker().Generate();

        catalog.Id = CatalogId.Of(Guid.NewGuid());

        dbContext.Catalogs.Add(catalog);
        await dbContext.SaveChangesAsync();

        GetCatalogByIdResult result = new GetCatalogByIdResult
        {
            Id = catalog.Id.Value,
            Name = catalog.Name,
            Description = catalog.Description,
            ImageUrl = catalog.ImageUrl,
            Price = catalog.Price.Value,
            Categories = catalog.Categories.Select(c => c).ToList()
        };

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<GetCatalogByIdQuery>()))
            .ReturnsAsync(Result<GetCatalogByIdResult>.Success(result));

        HttpRequestMessage request = new(HttpMethod.Get, $"/catalog/{catalog.Id.Value}");
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Result<GetCatalogByIdResult>? responseResult =
            await response.Content.ReadFromJsonAsync<Result<GetCatalogByIdResult>>();
        responseResult?.Data.Should().NotBeNull();
    }
}