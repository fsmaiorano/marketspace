using BuildingBlocks;
using BuildingBlocks.Loggers;
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
            logger.LogInformation(LogTypeEnum.Application, "Processing update order request for: {OrderId}", command.Id);
            
            // OrderEntity orderEntity = OrderEntity.Create(
            // );
            //
            // orderEntity.Id = OrderId.Of(command.Id);
            //
            // await repository.UpdateAsync(orderEntity);
            
            logger.LogInformation(LogTypeEnum.Business, "Order updated successfully. OrderId: {OrderId}", command.Id);

            return Result<UpdateOrderResult>.Success(new UpdateOrderResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while updating the order: {Command}", command);
            return Result<UpdateOrderResult>.Failure("An error occurred while updating the order.");
        }
    }
}