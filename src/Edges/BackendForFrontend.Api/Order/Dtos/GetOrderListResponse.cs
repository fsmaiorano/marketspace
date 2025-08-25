namespace BackendForFrontend.Api.Order.Dtos;

public class GetOrderListResponse
{
    public List<OrderSummaryDto> Orders { get; set; } = new();
}

public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
