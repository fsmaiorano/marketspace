namespace Merchant.Test.Application.Merchant.UpdateMerchant;

public class UpdateMerchantHandlerTest
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<IAppLogger<UpdateMerchantHandler>> loggerMock = new();

        MerchantId merchantId = MerchantId.Of(Guid.CreateVersion7());

        repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<MerchantEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        repositoryMock.Setup(r =>
                r.GetByIdAsync(It.IsAny<MerchantId>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => merchant);

        UpdateMerchantHandler handler = new UpdateMerchantHandler(repositoryMock.Object, loggerMock.Object);

        UpdateMerchantCommand command = new UpdateMerchantCommand
        {
            Id = merchantId.Value,
            Name = $"{merchant.Name}_Updated",
            Description = merchant.Description,
            Email = merchant.Email.Value,
            PhoneNumber = merchant.PhoneNumber,
            Address = merchant.Address
        };

        Result<UpdateMerchantResult> result = await handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }
}