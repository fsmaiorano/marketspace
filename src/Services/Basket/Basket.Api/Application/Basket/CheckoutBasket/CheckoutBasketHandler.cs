using System.Collections.Generic;
using Basket.Api.Application.Basket.CheckoutBasket.Contracts;
using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;

namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketHandler(
    IBasketDataRepository basketDataRepository,
    ICheckoutHttpRepository checkoutHttpRepository,
    IApplicationLogger<CheckoutBasketHandler> applicationLogger,
    IBusinessLogger<CheckoutBasketHandler> businessLogger)
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
            
            applicationLogger.LogInformation("Starting checkout process for user: {Username}", command.UserName);

            ShoppingCartEntity? basket = await basketDataRepository.GetCartAsync(command.UserName);

            if (basket is null || basket.Items.Count == 0)
            {
                applicationLogger.LogWarning("Basket not found or empty for user: {Username}", command.UserName);
                return Result<CheckoutBasketResult>.Failure("Basket is empty or does not exist.");
            }

            applicationLogger.LogInformation("Basket found with {ItemCount} items for user: {Username}",
                basket.Items.Count, command.UserName);

            List<OrderItemRequest> orderItems = basket.Items.Select(item => new OrderItemRequest
            {
                CatalogId = Guid.Parse(item.ProductId), Quantity = item.Quantity, Price = item.Price
            }).ToList();

            CreateOrderRequest orderRequest = new CreateOrderRequest
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

            applicationLogger.LogInformation("Creating order for customer: {CustomerId} with {ItemCount} items",
                command.CustomerId, orderItems.Count);

            CreateOrderResponse? orderResponse =
                await checkoutHttpRepository.CreateOrderAsync(orderRequest, idempotencyKey, correlationId);

            if (orderResponse == null)
            {
                applicationLogger.LogError("Failed to create order for customer: {CustomerId}", command.CustomerId);
                return Result<CheckoutBasketResult>.Failure("Failed to create order.");
            }

            bool checkoutSuccess = await basketDataRepository.CheckoutAsync(command.UserName);

            if (!checkoutSuccess)
            {
                applicationLogger.LogWarning(
                    "Order created but failed to clear basket for user: {Username}. OrderId: {OrderId}",
                    command.UserName, orderResponse.OrderId);
            }

            businessLogger.LogInformation(
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
            applicationLogger.LogError(ex, "An error occurred during checkout for user: {Username}", command.UserName);
            return Result<CheckoutBasketResult>.Failure($"Checkout failed: {ex.Message}");
        }
    }
}