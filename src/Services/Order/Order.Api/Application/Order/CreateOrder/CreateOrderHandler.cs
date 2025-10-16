using BuildingBlocks;
using BuildingBlocks.Loggers.Abstractions;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Application.Extensions;

namespace Order.Api.Application.Order.CreateOrder;

public sealed class CreateOrderHandler(
    IOrderRepository repository,
    IApplicationLogger<CreateOrderHandler> applicationLogger,
    IBusinessLogger<CreateOrderHandler> businessLogger
)
    : ICreateOrderHandler
{
    public async Task<Result<CreateOrderResult>> HandleAsync(CreateOrderCommand command)
    {
        try
        {
            applicationLogger.LogInformation("Processing create order request for customer: {CustomerId}", command.CustomerId);
            
            OrderId orderId = OrderId.Of(Guid.CreateVersion7());
            Address shippingAddress = command.ShippingAddress.ToAddress();
            Address billingAddress = command.BillingAddress.ToAddress();
            Payment payment = command.Payment.ToPayment();
            List<OrderItemEntity> orderItems = command.Items.Select(item => orderId.ToOrderItemEntity(item)).ToList();
            
            OrderEntity orderEntity = OrderEntity.Create(
                orderId: orderId,
                customerId: CustomerId.Of(command.CustomerId),
                shippingAddress: shippingAddress,
                billingAddress: billingAddress,
                payment: payment,
                items: orderItems
            );

            int result = await repository.AddAsync(orderEntity);

            if (result <= 0)
            {
                applicationLogger.LogError("Failed to persist order to database for customer: {CustomerId}", command.CustomerId);
                return Result<CreateOrderResult>.Failure("Failed to create order.");
            }

            businessLogger.LogInformation("Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, ItemCount: {ItemCount}", 
                orderEntity.Id.Value, 
                command.CustomerId, 
                orderEntity.TotalAmount.Value, 
                orderItems.Count);
            
            return Result<CreateOrderResult>.Success(new CreateOrderResult(orderEntity.Id.Value));
        }
        catch (Exception ex)
        {
            applicationLogger.LogError(ex, "An error occurred while creating the order for customer: {CustomerId}", command.CustomerId);
            return Result<CreateOrderResult>.Failure($"An error occurred while creating the order: {ex.Message}");
        }
    }
}