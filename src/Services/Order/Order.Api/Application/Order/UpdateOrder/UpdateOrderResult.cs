namespace Order.Api.Application.Order.UpdateOrder;

public class UpdateOrderResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}