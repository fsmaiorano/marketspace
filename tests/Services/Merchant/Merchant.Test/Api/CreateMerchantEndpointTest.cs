using System.Net.Http.Json;
using BuildingBlocks;
using FluentAssertions;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Domain.ValueObjects;
using Builder;
using Moq;

namespace Merchant.Test.Api;

public class CreateMerchantEndpointTest : IClassFixture<MerchantApiFactory>
{
    private readonly HttpClient _client;

    public CreateMerchantEndpointTest(MerchantApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Created_Successfully()
    {
        Mock<ICreateMerchantHandler> mockHandler = new Mock<ICreateMerchantHandler>();

        Guid merchantId = Guid.NewGuid();

        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        Result<CreateMerchantResult>
            result = Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchantId));

        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ReturnsAsync(result);

        Result<CreateMerchantResult> response = await mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value!.MerchantId.Should().Be(merchantId);
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        Mock<ICreateMerchantHandler> mockHandler = new Mock<ICreateMerchantHandler>();
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        Result<CreateMerchantResult> result = Result<CreateMerchantResult>.Failure("Validation error");
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ReturnsAsync(result);
        Result<CreateMerchantResult> response = await mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        Mock<ICreateMerchantHandler> mockHandler = new Mock<ICreateMerchantHandler>();
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Create_Merchant_Endpoint()
    {
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        HttpResponseMessage response = await _client.PostAsJsonAsync("/merchant", command);
        response.EnsureSuccessStatusCode();
        
        CreateMerchantResult? result = await response.Content.ReadFromJsonAsync<CreateMerchantResult>();
        result.Should().NotBeNull();
        result!.MerchantId.Should().NotBeEmpty();
        result.MerchantId.Should().NotBe(Guid.Empty);
    }
}