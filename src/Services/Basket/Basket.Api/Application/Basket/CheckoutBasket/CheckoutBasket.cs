using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Services.Correlation;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public record CheckoutBasketCommand(
    string UserName,
    Guid CustomerId,
    string? FirstName,
    string? LastName,
    string? EmailAddress,
    string? AddressLine,
    string? Country,
    string? State,
    string? ZipCode,
    string? Coordinates,
    string? CardName,
    string? CardNumber,
    string? Expiration,
    string? Cvv,
    int PaymentMethod
);

public record CheckoutBasketResult();

public class CheckoutBasket(
    IBasketDataRepository basketDataRepository,
    ICorrelationIdService correlationIdService,
    IAppLogger<CheckoutBasket> logger)

{
    public async Task<Result<CheckoutBasketResult>> HandleAsync(CheckoutBasketCommand command)
    {
        try
        {
            string correlationId = correlationIdService.GetCorrelationId();

            logger.LogInformation(LogTypeEnum.Application,
                "Starting checkout process for user: {Username}, CorrelationId: {CorrelationId}",
                command.UserName, correlationId);

            ShoppingCartEntity? basket = await basketDataRepository.GetCartAsync(command.UserName);

            if (basket is null || basket.Items.Count == 0)
            {
                logger.LogWarning(LogTypeEnum.Application,
                    "Basket not found or empty for user: {Username}, CorrelationId: {CorrelationId}",
                    command.UserName, correlationId);
                return Result<CheckoutBasketResult>.Failure("Basket is empty or does not exist.");
            }

            logger.LogInformation(LogTypeEnum.Application,
                "Basket found with {ItemCount} items for user: {Username}, CorrelationId: {CorrelationId}",
                basket.Items.Count, command.UserName, correlationId);

            CheckoutAddress? address = null;
            if (!string.IsNullOrEmpty(command.FirstName) || !string.IsNullOrEmpty(command.AddressLine))
            {
                address = CheckoutAddress.Create(
                    command.FirstName,
                    command.LastName,
                    command.EmailAddress,
                    command.AddressLine,
                    command.Country,
                    command.State,
                    command.ZipCode,
                    command.Coordinates
                );
            }

            CheckoutPayment? payment = null;
            if (!string.IsNullOrEmpty(command.CardName) || !string.IsNullOrEmpty(command.CardNumber))
            {
                payment = CheckoutPayment.Create(
                    command.CardName,
                    command.CardNumber,
                    command.Expiration,
                    command.Cvv,
                    command.PaymentMethod
                );
            }

            CheckoutData checkoutData = CheckoutData.Create(
                command.CustomerId,
                command.UserName,
                address,
                payment,
                correlationId
            );

            await basketDataRepository.CheckoutAsync(command.UserName, checkoutData);
            return Result<CheckoutBasketResult>.Success(new CheckoutBasketResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred during checkout for user: {Username}",
                command.UserName);
            return Result<CheckoutBasketResult>.Failure($"Checkout failed: {ex.Message}");
        }
    }
}