namespace BackendForFrontend.Api.Aggregations.Dtos;

public class CustomerDashboardResponse
{
    public Guid CustomerId { get; set; }
    public CustomerBasketSummary Basket { get; set; } = new();
    public List<CustomerOrderSummary> RecentOrders { get; set; } = new();
    public List<RecommendedProduct> RecommendedProducts { get; set; } = new();
}

public class CustomerBasketSummary
{
    public string Username { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public decimal TotalValue { get; set; }
}

public class CustomerOrderSummary
{
    public Guid Id { get; set; }
    public string OrderName { get; set; } = string.Empty;
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecommendedProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageFile { get; set; } = string.Empty;
}
