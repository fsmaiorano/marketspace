using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Application.Extensions;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.UpdateOrder;

public sealed class UpdateOrderHandler(
    IOrderRepository repository,
    IAppLogger<UpdateOrderHandler> logger)
    : IUpdateOrderHandler
{
    public async Task<Result<UpdateOrderResult>> HandleAsync(UpdateOrderCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing update order request for: {OrderId}",
                command.Id);

            // Buscar entidade existente rastreada
            OrderId orderId = OrderId.Of(command.Id);
            OrderEntity? orderEntity = await repository.GetByIdAsync(orderId, isTrackingEnabled: true, CancellationToken.None);
            
            if (orderEntity == null)
            {
                logger.LogWarning(LogTypeEnum.Application, "Order not found for update: {OrderId}", command.Id);
                return Result<UpdateOrderResult>.Failure($"Order with ID {command.Id} not found.");
            }
            
            // Usar método de domínio para atualizar
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