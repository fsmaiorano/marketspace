using Moq;
using Microsoft.Extensions.Logging;
using BuildingBlocks;
using Merchant.Api.Application.Merchant.CreateMerchant;
using Merchant.Api.Domain.Entities;
using Merchant.Api.Domain.Exceptions;
using Merchant.Api.Domain.Repositories;
using Merchant.Api.Domain.ValueObjects;
using MockFactory;

namespace Merchant.Api.UnitTest.Application.Merchant.CreateMerchant;

public class CreateMerchantHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnSuccessResult_WhenNoExceptionOccurs()
    {
        MerchantEntity? merchant = MerchantMockBuilder.CreateMerchantFaker("").Generate();
        Mock<IMerchantRepository> repositoryMock = new();
        Mock<ILogger<CreateMerchantHandler>> loggerMock = new();
        Mock<CreateMerchantHandler> handlerMock = new(repositoryMock.Object, loggerMock.Object);

        repositoryMock.Setup(r => r.AddAsync(It.IsAny<MerchantEntity>())).ReturnsAsync(1);
        handlerMock.Setup(h => h.HandleAsync(It.IsAny<CreateMerchantCommand>()))
            .ReturnsAsync(Result<CreateMerchantResult>.Success(new CreateMerchantResult(merchant.Id.Value)));
        
        CreateMerchantHandler handler = handlerMock.Object;

        CreateMerchantCommand command = new(
            merchant.Name,
            merchant.Description,
            merchant.Address,
            merchant.PhoneNumber,
            merchant.Email);

        // Act
        Result<CreateMerchantResult> result = await handler.HandleAsync(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.MerchantId);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnFailureResult_WhenExceptionIsThrown()
    {
        MerchantEntity? merchant = MerchantMockBuilder.CreateMerchantFaker(email: "wrong-email-format").Generate();
        Mock<IMerchantRepository> repositoryMock = new Mock<IMerchantRepository>();
        Mock<ILogger<CreateMerchantHandler>> loggerMock = new Mock<ILogger<CreateMerchantHandler>>();

        repositoryMock.Setup(r => r.AddAsync(It.IsAny<MerchantEntity>())).ReturnsAsync(1);

        CreateMerchantHandler handler = new CreateMerchantHandler(repositoryMock.Object, loggerMock.Object);
        CreateMerchantCommand command = new CreateMerchantCommand(
            merchant.Name,
            merchant.Description,
            merchant.Address,
            merchant.PhoneNumber,
            merchant.Email);

        // Act
        Result<CreateMerchantResult> result = await handler.HandleAsync(command);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid email format", result.Error);
    }
}