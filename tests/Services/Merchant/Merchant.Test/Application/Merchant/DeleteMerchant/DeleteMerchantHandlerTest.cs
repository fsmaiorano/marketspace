using Merchant.Api.Application.Merchant.DeleteMerchant;

namespace Merchant.Test.Application.Merchant.DeleteMerchant;

public class DeleteMerchantHandlerTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<IAppLogger<DeleteMerchantHandler>> loggerMock = new();

        repositoryMock
            .Setup(r => r.RemoveAsync(It.IsAny<MerchantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        DeleteMerchantHandler handler = new DeleteMerchantHandler(repositoryMock.Object, loggerMock.Object);

        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker().Generate();

        Result<DeleteMerchantResult> result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}