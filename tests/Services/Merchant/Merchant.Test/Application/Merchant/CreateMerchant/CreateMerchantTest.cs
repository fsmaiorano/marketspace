namespace Merchant.Test.Application.Merchant.CreateMerchant;

public class CreateMerchantTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<IAppLogger<Api.Application.Merchant.CreateMerchant.CreateMerchant>> loggerMock = new();

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<MerchantEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantEntity m, CancellationToken _) =>
            {
                m.Id = MerchantId.Of(Guid.CreateVersion7());
                return 1;
            });

        Api.Application.Merchant.CreateMerchant.CreateMerchant handler = new(repositoryMock.Object, loggerMock.Object);

        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();

        Result<CreateMerchantResult> result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}