using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Application.Dto;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Application.Extensions;
using Order.Api.Domain.Enums;

namespace Order.Api.Application.Order.CreateOrder;

public record CreateOrderCommand
{
    public Guid CustomerId { get; init; } = Guid.Empty;
    public AddressDto ShippingAddress { get; init; } = null!;
    public AddressDto BillingAddress { get; init; } = null!;
    public PaymentDto Payment { get; init; } = null!;
    public OrderStatusEnum Status { get; init; } = OrderStatusEnum.Created;
    public List<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; } = 0.0m;
    public string? CorrelationId { get; init; }
}

public record CreateOrderResult();

public sealed class CreateOrder(
    IOrderRepository repository,
    IAppLogger<CreateOrder> logger
)
{
    public async Task<Result<CreateOrderResult>> HandleAsync(CreateOrderCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application,
                "Processing create order request for customer: {CustomerId}, CorrelationId: {CorrelationId}",
                command.CustomerId, command.CorrelationId);

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
                items: orderItems,
                status: OrderStatusEnum.Created,
                correlationId: command.CorrelationId
            );

            int result = await repository.AddAsync(orderEntity);

            if (result <= 0)
            {
                logger.LogError(LogTypeEnum.Application, null,
                    "Failed to persist order to database for customer: {CustomerId}", command.CustomerId);
                return Result<CreateOrderResult>.Failure("Failed to create order.");
            }

            logger.LogInformation(LogTypeEnum.Business,
                "Order created successfully. OrderId: {OrderId}, CustomerId: {CustomerId}, TotalAmount: {TotalAmount}, ItemCount: {ItemCount}, CorrelationId: {CorrelationId}",
                orderEntity.Id.Value,
                command.CustomerId,
                orderEntity.TotalAmount.Value,
                orderItems.Count,
                command.CorrelationId);

            return Result<CreateOrderResult>.Success(new CreateOrderResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex,
                "An error occurred while creating the order for customer: {CustomerId}", command.CustomerId);
            return Result<CreateOrderResult>.Failure($"An error occurred while creating the order: {ex.Message}");
        }
    }
}