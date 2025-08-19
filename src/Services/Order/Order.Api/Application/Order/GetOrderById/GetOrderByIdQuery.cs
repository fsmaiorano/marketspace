namespace Order.Api.Application.Order.GetOrderById;

public record GetOrderByIdQuery(Guid Id) 
{
    public Guid Id { get; init; } = Id;
}