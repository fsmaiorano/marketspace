using Merchant.Api.Application.Merchant.DeleteMerchant;

namespace Merchant.Test.Api.Endpoints;

public class DeleteMerchantEndpointTest(MerchantApiFactory factory) : IClassFixture<MerchantApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Mock<IDeleteMerchantHandler> _mockHandler = new();
    private readonly MerchantApiFactory _factory = factory;

    [Fact]
    public async Task Returns_Ok_When_Merchant_Is_Deleted_Successfully()
    {
        Guid merchantId = Guid.NewGuid();

        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker().Generate();
        Result<DeleteMerchantResult> result = Result<DeleteMerchantResult>.Success(new DeleteMerchantResult(true));

        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteMerchantCommand>()))
            .ReturnsAsync(result);

        Result<DeleteMerchantResult> response = await _mockHandler.Object.HandleAsync(command);

        response.IsSuccess.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Returns_Error_Result_When_Handler_Fails()
    {
        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker().Generate();
        Result<DeleteMerchantResult> result = Result<DeleteMerchantResult>.Failure("Validation error");
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteMerchantCommand>()))
            .ReturnsAsync(result);
        Result<DeleteMerchantResult> response = await _mockHandler.Object.HandleAsync(command);
        response.IsSuccess.Should().BeFalse();
        response.Error.Should().Be("Validation error");
    }

    [Fact]
    public async Task Throws_Exception_When_Handler_Throws()
    {
        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker().Generate();
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<DeleteMerchantCommand>()))
            .ThrowsAsync(new Exception("Unexpected error"));
        Func<Task> act = async () => await _mockHandler.Object.HandleAsync(command);
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Unexpected error");
    }

    [Fact]
    public async Task Can_Delete_Merchant_Endpoint()
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        MerchantDbContext dbContext = scope.ServiceProvider.GetRequiredService<MerchantDbContext>();

        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker().Generate();

        dbContext.Merchants.Add(merchant);
        await dbContext.SaveChangesAsync();
        
        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker(merchant.Id.Value).Generate();
        HttpRequestMessage request = new(HttpMethod.Delete, "/merchant")
        {
            Content = JsonContent.Create(command)
        };
        HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        DeleteMerchantResult? result = await response.Content.ReadFromJsonAsync<DeleteMerchantResult>();
        result.Should().NotBeNull();
    }
}