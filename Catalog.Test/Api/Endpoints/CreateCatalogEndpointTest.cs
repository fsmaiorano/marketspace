using Builder;
using BuildingBlocks;
using Catalog.Api.Application.Catalog.CreateCatalog;
using FluentAssertions;
using Moq;
using System.Net.Http.Json;

namespace Catalog.Test.Api.Endpoints;

public class CreateCatalogEndpointTest(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<ICreateCatalogHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Catalog_Is_Created_Successfully()
    {
        Guid merchantId = Guid.NewGuid();

        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        Result<CreateCatalogResult>
            result = Result<CreateCatalogResult>.Success(new CreateCatalogResult(merchantId));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateCatalogCommand>()))
            .ReturnsAsync(result);

        Result<CreateCatalogResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        Result<CreateCatalogResult> result = Result<CreateCatalogResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateCatalogCommand>()))
            .ReturnsAsync(result);
        Result<CreateCatalogResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateCatalogCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Create_Catalog_Endpoint()
    {
        CreateCatalogCommand command = CatalogBuilder.CreateCreateCatalogCommandFaker().Generate();
        
        HttpResponseMessage response = await _client.PostAsJsonAsync("/catalog", command);
        response.EnsureSuccessStatusCode();

        CreateCatalogResult? result = await response.Content.ReadFromJsonAsync<CreateCatalogResult>();
        result.Should().NotBeNull();
        result!.CatalogId.Should().NotBeEmpty();
        result.CatalogId.Should().NotBe(Guid.Empty);
    }
}