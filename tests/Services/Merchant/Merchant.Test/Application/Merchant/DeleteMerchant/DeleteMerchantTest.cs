using Merchant.Api.Application.Merchant.DeleteMerchant;

namespace Merchant.Test.Application.Merchant.DeleteMerchant;

public class DeleteMerchantTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<IAppLogger<Api.Application.Merchant.DeleteMerchant.DeleteMerchant>> loggerMock = new();

        repositoryMock
            .Setup(r => r.RemoveAsync(It.IsAny<MerchantId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        Api.Application.Merchant.DeleteMerchant.DeleteMerchant handler = new(repositoryMock.Object, loggerMock.Object);

        DeleteMerchantCommand command = MerchantBuilder.CreateDeleteMerchantCommandFaker();

        Result<DeleteMerchantResult> result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}