namespace Merchant.Test.Api;

public class CreateMerchantEndpointTest(MerchantApiFactory factory) : IClassFixture<MerchantApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<ICreateMerchantHandler> _mockHandler = new();

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Created_Successfully()
    {
        Guid merchantId = Guid.NewGuid();

        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        Result<CreateMerchantResult>
            result = Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchantId));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ReturnsAsync(result);

        Result<CreateMerchantResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Value.Should().NotBeNull();
        response.Value!.MerchantId.Should().Be(merchantId);
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        Result<CreateMerchantResult> result = Result<CreateMerchantResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ReturnsAsync(result);
        Result<CreateMerchantResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
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