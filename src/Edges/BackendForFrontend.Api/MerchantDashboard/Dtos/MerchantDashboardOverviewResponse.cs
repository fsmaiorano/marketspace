namespace BackendForFrontend.Api.MerchantDashboard.Dtos;

public class MerchantDashboardOverviewResponse
{
    public MerchantDashboardSummaryDto Summary { get; set; } = new();
    public List<MerchantDashboardOrderDto> RecentOrders { get; set; } = new();
    public List<MerchantDashboardProductSalesDto> ProductSales { get; set; } = new();
    public DateTimeOffset GeneratedAt { get; set; }
}

public class MerchantDashboardSummaryDto
{
    public int TotalOrders { get; set; }
    public int TotalUnitsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ProcessingOrders { get; set; }
    public int CompletedOrders { get; set; }
}

public class MerchantDashboardOrderDto
{
    public Guid OrderId { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal MerchantTotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<MerchantDashboardOrderItemDto> Items { get; set; } = new();
}

public class MerchantDashboardOrderItemDto
{
    public Guid CatalogId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class MerchantDashboardProductSalesDto
{
    public Guid CatalogId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public int OrderCount { get; set; }
    public int CurrentStock { get; set; }
}
