namespace BackendForFrontend.Api.Aggregations.Dtos;

public class OrderSummaryResponse
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDetail> Items { get; set; } = new();
    public OrderAddressInfo ShippingAddress { get; set; } = new();
}

public class OrderItemDetail
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderAddressInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}
