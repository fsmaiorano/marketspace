namespace Order.Api.Application.Order.DeleteOrder;

public class DeleteOrderResult(bool isSuccess)
{
    public bool IsSuccess { get; init; } = isSuccess;
}