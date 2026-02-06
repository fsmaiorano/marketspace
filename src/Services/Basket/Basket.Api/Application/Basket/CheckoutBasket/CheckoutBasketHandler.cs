using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using Basket.Api.Domain.ValueObjects;
using BuildingBlocks;
using BuildingBlocks.Loggers;
using BuildingBlocks.Services.Correlation;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketHandler(
    IBasketDataRepository basketDataRepository,
    ICorrelationIdService correlationIdService,
    IAppLogger<CheckoutBasketHandler> logger)
    : ICheckoutBasketHandler
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

            // Create Address Value Object only if address fields are provided
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

            // Create Payment Value Object only if payment fields are provided
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
       
            // Create CheckoutData Value Object
            CheckoutData checkoutData = CheckoutData.Create(
                command.CustomerId,
                command.UserName,
                address,
                payment,
                correlationId
            );

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

