using System.Collections.Generic;
using Basket.Api.Application.Basket.CheckoutBasket.Contracts;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;
using BuildingBlocks.Loggers;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketHandler(
    IBasketDataRepository basketDataRepository,
    ICheckoutHttpRepository checkoutHttpRepository,
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

            logger.LogInformation(LogTypeEnum.Application, "Starting checkout process for user: {Username}", command.UserName);

            ShoppingCartEntity? basket = await basketDataRepository.GetCartAsync(command.UserName);

            if (basket is null || basket.Items.Count == 0)
            {
                logger.LogWarning(LogTypeEnum.Application, "Basket not found or empty for user: {Username}", command.UserName);
                return Result<CheckoutBasketResult>.Failure("Basket is empty or does not exist.");
            }

            logger.LogInformation(LogTypeEnum.Application, "Basket found with {ItemCount} items for user: {Username}",
                basket.Items.Count, command.UserName);

            List<OrderItemRequest> orderItems = basket.Items.Select(item => new OrderItemRequest
            {
                CatalogId = Guid.Parse(item.ProductId),
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList();

            CreateOrderRequest orderRequest = new()

            {
                CustomerId = command.CustomerId,
                ShippingAddress =
                    new AddressRequest
                    {
                        FirstName = command.FirstName,
                        LastName = command.LastName,
                        EmailAddress = command.EmailAddress,
                        AddressLine = command.AddressLine,
                        Country = command.Country,
                        State = command.State,
                        ZipCode = command.ZipCode
                    },
                BillingAddress = new AddressRequest
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    EmailAddress = command.EmailAddress,
                    AddressLine = command.AddressLine,
                    Country = command.Country,
                    State = command.State,
                    ZipCode = command.ZipCode
                },
                Payment = new PaymentRequest
                {
                    CardName = command.CardName,
                    CardNumber = command.CardNumber,
                    Expiration = command.Expiration,
                    Cvv = command.Cvv,
                    PaymentMethod = command.PaymentMethod
                },
                Items = orderItems
            };

            logger.LogInformation(LogTypeEnum.Application, "Creating order for customer: {CustomerId} with {ItemCount} items",
                command.CustomerId, orderItems.Count);

            CreateOrderResponse? orderResponse =
                await checkoutHttpRepository.CreateOrderAsync(orderRequest, idempotencyKey, correlationId);

            if (orderResponse == null)
            {
                logger.LogError(LogTypeEnum.Application, null, "Failed to create order for customer: {CustomerId}", command.CustomerId);
                return Result<CheckoutBasketResult>.Failure("Failed to create order.");
            }

            bool checkoutSuccess = await basketDataRepository.CheckoutAsync(command.UserName);

            if (!checkoutSuccess)
            {
                logger.LogWarning(LogTypeEnum.Application,
                    "Order created but failed to clear basket for user: {Username}. OrderId: {OrderId}",
                    command.UserName, orderResponse.OrderId);
            }

            logger.LogInformation(LogTypeEnum.Business,
                "Checkout completed successfully. Username: {Username}, CustomerId: {CustomerId}, OrderId: {OrderId}, TotalAmount: {TotalAmount}, ItemCount: {ItemCount}, CorrelationId: {CorrelationId}",
                command.UserName,
                command.CustomerId,
                orderResponse.OrderId,
                basket.TotalPrice,
                basket.Items.Count,
                correlationId);

            return Result<CheckoutBasketResult>.Success(new CheckoutBasketResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred during checkout for user: {Username}", command.UserName);
            return Result<CheckoutBasketResult>.Failure($"Checkout failed: {ex.Message}");
        }
    }
}