using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Application.Extensions;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;
using Order.Api.Endpoints.Dto;

namespace Order.Api.Application.Order.UpdateOrder;

public record UpdateOrderCommand
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid CustomerId { get; set; } = Guid.Empty;
    public AddressDto ShippingAddress { get; set; } = null!;
    public AddressDto BillingAddress { get; set; } = null!;
    public PaymentDto Payment { get; set; } = null!;
    public OrderStatusEnum Status { get; set; } = OrderStatusEnum.Created;
    public List<OrderItemDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; } = 0.0m;
}

public record UpdateOrderResult();

public sealed class UpdateOrder(
    IOrderRepository repository,
    IAppLogger<UpdateOrder> logger)
{
    public async Task<Result<UpdateOrderResult>> HandleAsync(UpdateOrderCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing update order request for: {OrderId}",
                command.Id);

            OrderId orderId = OrderId.Of(command.Id);
            OrderEntity? orderEntity =
                await repository.GetByIdAsync(orderId, isTrackingEnabled: true, CancellationToken.None);

            if (orderEntity == null)
            {
                logger.LogWarning(LogTypeEnum.Application, "Order not found for update: {OrderId}", command.Id);
                return Result<UpdateOrderResult>.Failure($"Order with ID {command.Id} not found.");
            }

            orderEntity.Update(
                command.ShippingAddress.ToAddress(),
                command.BillingAddress.ToAddress(),
                command.Payment.ToPayment(),
                command.Status,
                command.Items.ToOrderItems(orderId));

            await repository.UpdateAsync(orderEntity);

            logger.LogInformation(LogTypeEnum.Business, "Order updated successfully. OrderId: {OrderId}", command.Id);

            return Result<UpdateOrderResult>.Success(new UpdateOrderResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while updating the order: {Command}",
                command);
            return Result<UpdateOrderResult>.Failure("An error occurred while updating the order.");
        }
    }
}