using Basket.Api.Application.Basket.CheckoutBasket.Dtos;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
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
            // Get CorrelationId from the service (set by middleware or generate new one)
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

            // Create Address DTO only if address fields are provided
            CheckoutAddressDto? address = null;
            if (!string.IsNullOrEmpty(command.FirstName) || !string.IsNullOrEmpty(command.AddressLine))
            {
                address = new CheckoutAddressDto
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    EmailAddress = command.EmailAddress,
                    AddressLine = command.AddressLine,
                    Country = command.Country,
                    State = command.State,
                    ZipCode = command.ZipCode,
                    Coordinates = command.Coordinates
                };
            }

            // Create Payment DTO only if payment fields are provided
            CheckoutPaymentDto? payment = null;
            if (!string.IsNullOrEmpty(command.CardName) || !string.IsNullOrEmpty(command.CardNumber))
            {
                payment = new CheckoutPaymentDto
                {
                    CardName = command.CardName,
                    CardNumber = command.CardNumber,
                    Expiration = command.Expiration,
                    Cvv = command.Cvv,
                    PaymentMethod = command.PaymentMethod
                };
            }
       
            CheckoutDataDto checkoutData = new()
            {
                CustomerId = command.CustomerId,
                UserName = command.UserName,
                Address = address,
                Payment = payment,
                CorrelationId = correlationId
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

