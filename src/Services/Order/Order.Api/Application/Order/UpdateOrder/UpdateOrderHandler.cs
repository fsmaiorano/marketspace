using BuildingBlocks;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.UpdateOrder;

public sealed class UpdateOrderHandler(IOrderRepository repository, ILogger<UpdateOrderHandler> logger)
    : IUpdateOrderHandler
{
    public async Task<Result<UpdateOrderResult>> HandleAsync(UpdateOrderCommand command)
    {
        try
        {
            // OrderEntity orderEntity = OrderEntity.Create(
            // );
            //
            // orderEntity.Id = OrderId.Of(command.Id);
            //
            // await repository.UpdateAsync(orderEntity);
            logger.LogInformation("Order updated successfully: {OrderId}", command.Id);

            return Result<UpdateOrderResult>.Success(new UpdateOrderResult(true));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating the catalog: {Command}", command);
            return Result<UpdateOrderResult>.Failure("An error occurred while updating the catalog.");
        }
    }
}