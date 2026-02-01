using Basket.Api.Application.Basket.CheckoutBasket.Dtos;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketHandler(
    IBasketDataRepository basketDataRepository,
    IAppLogger<CheckoutBasketHandler> logger)
    : ICheckoutBasketHandler
{
    public async Task<Result<CheckoutBasketResult>> HandleAsync(CheckoutBasketCommand command)
    {
        try
        {
            string correlationId = !string.IsNullOrWhiteSpace(command.RequestId)
                ? command.RequestId!
                : Guid.NewGuid().ToString();

            string? idempotencyKey = !string.IsNullOrWhiteSpace(command.IdempotencyKey)
                ? command.IdempotencyKey
                : command.RequestId;

            logger.LogInformation(LogTypeEnum.Application, "Starting checkout process for user: {Username}",
                command.UserName);

            ShoppingCartEntity? basket = await basketDataRepository.GetCartAsync(command.UserName);

            if (basket is null || basket.Items.Count == 0)
            {
                logger.LogWarning(LogTypeEnum.Application, "Basket not found or empty for user: {Username}",
                    command.UserName);
                return Result<CheckoutBasketResult>.Failure("Basket is empty or does not exist.");
            }

            logger.LogInformation(LogTypeEnum.Application, "Basket found with {ItemCount} items for user: {Username}",
                basket.Items.Count, command.UserName);

       
            CheckoutDataDto checkoutData = new()
            {
                CustomerId = command.CustomerId,
                UserName = command.UserName,
                Address = new CheckoutAddressDto
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    EmailAddress = command.EmailAddress,
                    AddressLine = command.AddressLine,
                    Country = command.Country,
                    State = command.State,
                    ZipCode = command.ZipCode
                },
                Payment = new CheckoutPaymentDto
                {
                    CardName = command.CardName,
                    CardNumber = command.CardNumber,
                    Expiration = command.Expiration,
                    Cvv = command.Cvv,
                    PaymentMethod = command.PaymentMethod
                },
                CorrelationId = correlationId,
                IdempotencyKey = idempotencyKey
            };

            await basketDataRepository.CheckoutAsync(command.UserName, checkoutData);
            
            return Result<CheckoutBasketResult>.Success(new CheckoutBasketResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred during checkout for user: {Username}",
                command.UserName);
            return Result<CheckoutBasketResult>.Failure($"Checkout failed: {ex.Message}");
        }
    }
}

