using WebApp.Services.Abstractions;

namespace WebApp.Services;

/// <summary>
/// Example of another SSE service implementation for Order processing
/// Demonstrates how the generic SSE architecture can be used for different domains
/// </summary>
public class OrderSSEService : SSEServiceBase<OrderRequest, OrderResult>
{
    public OrderSSEService(ILogger<OrderSSEService> logger) : base(logger)
    {
    }

    protected override async Task ExecuteOperationAsync(OperationStatus<OrderResult> operation)
    {
        try
        {
            UpdateOperation(operation.Id, "Processing", 5, "Initializing order operation...");

            switch (operation.Type.ToLower())
            {
                case "create-order":
                    await ExecuteCreateOrderOperation(operation);
                    break;
                case "process-payment":
                    await ExecutePaymentOperation(operation);
                    break;
                case "fulfillment":
                    await ExecuteFulfillmentOperation(operation);
                    break;
                default:
                    await ExecuteGenericOrderOperation(operation);
                    break;
            }

            UpdateOperation(operation.Id, "Completed", 100, "Order operation completed successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Order operation {OperationId} failed", operation.Id);
            UpdateOperation(operation.Id, "Failed", operation.Progress, ex.Message);
        }
    }

    private async Task ExecuteCreateOrderOperation(OperationStatus<OrderResult> operation)
    {
        var orderItems = new List<OrderItem>();
        
        UpdateOperation(operation.Id, "Processing", 10, "Validating order items...");
        await Task.Delay(600);

        UpdateOperation(operation.Id, "Processing", 25, "Checking inventory availability...");
        await Task.Delay(800);

        UpdateOperation(operation.Id, "Processing", 40, "Calculating totals and taxes...");
        await Task.Delay(500);

        UpdateOperation(operation.Id, "Processing", 60, "Creating order record...");
        
        // Mock order creation
        var orderId = Guid.NewGuid().ToString();
        orderItems.Add(new OrderItem
        {
            ProductId = 1,
            ProductName = "Sample Product",
            Quantity = 2,
            Price = 29.99m
        });

        var result = new OrderResult
        {
            OrderId = orderId,
            Items = orderItems,
            TotalAmount = 59.98m,
            Status = "Created",
            CreatedAt = DateTime.UtcNow
        };
        UpdateOperationResult(operation.Id, result);
        
        await Task.Delay(700);

        UpdateOperation(operation.Id, "Processing", 85, "Sending confirmation email...");
        await Task.Delay(400);

        UpdateOperation(operation.Id, "Processing", 95, "Finalizing order...");
        await Task.Delay(300);
    }

    private async Task ExecutePaymentOperation(OperationStatus<OrderResult> operation)
    {
        UpdateOperation(operation.Id, "Processing", 15, "Connecting to payment gateway...");
        await Task.Delay(500);

        UpdateOperation(operation.Id, "Processing", 30, "Validating payment method...");
        await Task.Delay(600);

        UpdateOperation(operation.Id, "Processing", 50, "Processing payment...");
        await Task.Delay(1000);

        UpdateOperation(operation.Id, "Processing", 75, "Confirming transaction...");
        await Task.Delay(400);

        var result = new OrderResult
        {
            OrderId = "ORD-12345",
            PaymentStatus = "Completed",
            TransactionId = Guid.NewGuid().ToString(),
            TotalAmount = 99.99m,
            ProcessedAt = DateTime.UtcNow
        };
        UpdateOperationResult(operation.Id, result);

        UpdateOperation(operation.Id, "Processing", 90, "Updating order status...");
        await Task.Delay(300);
    }

    private async Task ExecuteFulfillmentOperation(OperationStatus<OrderResult> operation)
    {
        UpdateOperation(operation.Id, "Processing", 20, "Preparing items for shipment...");
        await Task.Delay(700);

        UpdateOperation(operation.Id, "Processing", 40, "Generating shipping labels...");
        await Task.Delay(500);

        UpdateOperation(operation.Id, "Processing", 60, "Packaging items...");
        await Task.Delay(800);

        UpdateOperation(operation.Id, "Processing", 80, "Scheduling pickup...");
        await Task.Delay(400);

        var result = new OrderResult
        {
            OrderId = "ORD-12345",
            TrackingNumber = "TRK-" + Guid.NewGuid().ToString()[..8].ToUpper(),
            ShippingStatus = "Shipped",
            EstimatedDelivery = DateTime.UtcNow.AddDays(3),
            ShippedAt = DateTime.UtcNow
        };
        UpdateOperationResult(operation.Id, result);

        await Task.Delay(300);
    }

    private async Task ExecuteGenericOrderOperation(OperationStatus<OrderResult> operation)
    {
        for (int i = 1; i <= 6; i++)
        {
            await Task.Delay(500);
            var progress = 10 + (i * 15);
            UpdateOperation(operation.Id, "Processing", progress, $"Order processing step {i}/6");
        }
    }
}

// Data models for order operations
public class OrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? PaymentMethodId { get; set; }
    public ShippingAddress? ShippingAddress { get; set; }
    public string? OrderId { get; set; }
    public decimal? Amount { get; set; }
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class ShippingAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class OrderResult
{
    public string OrderId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
    public string? TransactionId { get; set; }
    public string? TrackingNumber { get; set; }
    public string? ShippingStatus { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
