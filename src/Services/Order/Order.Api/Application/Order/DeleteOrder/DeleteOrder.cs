using BuildingBlocks;
using BuildingBlocks.Loggers;
using Order.Api.Domain.Repositories;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Order.DeleteOrder;

public record DeleteOrderCommand(Guid Id);
  
public record DeleteOrderResult();

public class DeleteOrder(
    IOrderRepository repository, 
    IAppLogger<DeleteOrder> logger)
{
    public async Task<Result<DeleteOrderResult>> HandleAsync(DeleteOrderCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing delete order request for: {OrderId}", command.Id);
            
            OrderId orderId = OrderId.Of(command.Id);

            await repository.RemoveAsync(orderId);
            
            logger.LogInformation(LogTypeEnum.Business, "Order deleted successfully. OrderId: {OrderId}", command.Id);
            return Result<DeleteOrderResult>.Success(new DeleteOrderResult());
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while deleting the order: {Command}", command);
            return Result<DeleteOrderResult>.Failure("An error occurred while deleting the order.");
        }
    }
}