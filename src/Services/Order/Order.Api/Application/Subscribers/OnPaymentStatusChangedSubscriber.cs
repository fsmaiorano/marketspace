using BuildingBlocks.Loggers;
using BuildingBlocks.Messaging.IntegrationEvents;
using BuildingBlocks.Messaging.IntegrationEvents.Interfaces;
using Order.Api.Application.Mapping;
using Order.Api.Application.Order.PatchOrderStatus;
using Order.Api.Domain.Enums;

namespace Order.Api.Application.Subscribers;

public class OnPaymentStatusChangedSubscriber(
    IAppLogger<OnPaymentStatusChangedSubscriber> logger,
    PatchOrderStatus patchOrderStatusHandler) : IIntegrationEventHandler<PaymentStatusChangedIntegrationEvent>
{
    public async Task HandleAsync(PaymentStatusChangedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(LogTypeEnum.Application,
            "Payment status changed event received. OrderId: {OrderId}, PaymentStatus: {PaymentStatus} ({PaymentStatusName})",
            @event.OrderId, @event.PaymentStatus, @event.PaymentStatusName);

        try
        {
            OrderStatusEnum orderStatus = PaymentStatusMapper.ToOrderStatus(@event.PaymentStatus);

            await patchOrderStatusHandler.HandleAsync(new PatchOrderStatusCommand
            {
                Id = @event.OrderId, Status = orderStatus
            });

            logger.LogInformation(LogTypeEnum.Application,
                "Order status updated successfully. OrderId: {OrderId}, NewStatus: {OrderStatus}",
                @event.OrderId, orderStatus);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogError(LogTypeEnum.Application,
                ex,
                "Unknown payment status received. OrderId: {OrderId}, PaymentStatus: {PaymentStatus}",
                @event.OrderId, @event.PaymentStatus);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Application,
                ex,
                "Failed to update order status. OrderId: {OrderId}",
                @event.OrderId);
            throw;
        }
    }
}