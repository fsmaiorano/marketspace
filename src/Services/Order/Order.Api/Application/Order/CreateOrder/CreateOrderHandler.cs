using BuildingBlocks;
using Order.Api.Application.Dto;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Application.Extensions;

namespace Order.Api.Application.Order.CreateOrder;

public sealed class CreateOrderHandler(
    IOrderRepository repository,
    ILogger<CreateOrderHandler> logger
)
    : ICreateOrderHandler
{
    public async Task<Result<CreateOrderResult>> HandleAsync(CreateOrderCommand command)
    {
        try
        {
            Address shippingAddress = command.ShippingAddress.ToAddress();
            Address billingAddress = command.BillingAddress.ToAddress();
            Payment payment = command.Payment.ToPayment();
            List<OrderItemEntity> orderItems = [];
            orderItems.AddRange(command.Items.Select(orderItem => OrderItemEntity.Create(orderId: null,
                catalogId: CatalogId.Of(orderItem.CatalogId), quantity: orderItem.Quantity,
                price: Price.Of(orderItem.Price))));

            OrderEntity orderEntity = OrderEntity.Create(
                customerId: CustomerId.Of(command.CustomerId),
                shippingAddress: shippingAddress,
                billingAddress: billingAddress,
                payment: payment,
                items: orderItems
            );

            int result = await repository.AddAsync(orderEntity);

            if (result <= 0)
            {
                logger.LogError("Failed to create order: {Command}", command);
                return Result<CreateOrderResult>.Failure("Failed to create order.");
            }

            logger.LogInformation("Order created successfully: {OrderId}", orderEntity.Id);
            return Result<CreateOrderResult>.Success(new CreateOrderResult(orderEntity.Id.Value));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating the order: {Command}", command);
            return Result<CreateOrderResult>.Failure($"An error occurred while creating the order: {ex.Message}");
        }
    }
}