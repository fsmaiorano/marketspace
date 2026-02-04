using BuildingBlocks;

namespace Order.Api.Application.Order.PatchOrderStatus;

public interface IPatchOrderStatusHandler
{
    Task<Result<PatchOrderStatusResult>> HandleAsync(PatchOrderStatusCommand command);
}
