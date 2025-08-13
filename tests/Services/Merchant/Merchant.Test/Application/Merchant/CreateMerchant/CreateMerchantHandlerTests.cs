using Moq;
using Microsoft.Extensions.Logging;
using BuildingBlocks;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Exceptions;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;
using Builder;

namespace Merchant.Test.Application.Merchant.CreateMerchant;

public class CreateMerchantHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<ILogger<CreateMerchantHandler>> loggerMock = new();

        repositoryMock.Setup(r => r.AddAsync(It.IsAny<MerchantEntity>())).ReturnsAsync((MerchantEntity m) =>
        {
            m.Id = MerchantId.Of(Guid.NewGuid());
            return 1;
        });

        CreateMerchantHandler handler = new CreateMerchantHandler(repositoryMock.Object, loggerMock.Object);

        CreateMerchantCommand command = MerchantBuilder.CreateCreateMerchantCommandFaker().Generate();

        // Act
        Result<CreateMerchantResult> result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.MerchantId);
    }

    [Fact]
    public Task HandleAsync_ShouldReturnFailureResult_WhenExceptionIsThrown()
    {
        try
        {
            MerchantEntity? merchant = MerchantBuilder.CreateMerchantFaker(email: "wrong-email-format").Generate();
        }
        catch (DomainException ex)
        {
            Assert.IsType<DomainException>(ex);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Unexpected exception type: {ex.GetType()}");
        }

        return Task.CompletedTask;
    }
}