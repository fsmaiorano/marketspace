using Microsoft.AspNetCore.Mvc;
using WebApp.Controllers.Abstractions;
using WebApp.Services;

namespace WebApp.Controllers;

/// <summary>
/// Order controller demonstrating how different domains can use the same SSE architecture
/// Shows the flexibility and reusability of the generic SSE base classes
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class OrderController : SSEControllerBase<OrderSSEService, OrderParameters, OrderResult>
{
    public OrderController(OrderSSEService orderService, ILogger<OrderController> logger) 
        : base(orderService, logger)
    {
    }

    /// <summary>
    /// Create a new order with SSE progress tracking
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var parameters = new OrderParameters
        {
            Items = request.Items,
            ShippingAddress = request.ShippingAddress
        };

        var operationRequest = new OperationRequest<OrderParameters>
        {
            OperationType = "create-order",
            Parameters = parameters
        };

        return await StartOperation(operationRequest);
    }

    /// <summary>
    /// Process payment for an order
    /// </summary>
    [HttpPost("process-payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
    {
        var parameters = new OrderParameters
        {
            OrderId = request.OrderId,
            PaymentMethodId = request.PaymentMethodId,
            Amount = request.Amount
        };

        var operationRequest = new OperationRequest<OrderParameters>
        {
            OperationType = "process-payment",
            Parameters = parameters
        };

        return await StartOperation(operationRequest);
    }

    /// <summary>
    /// Start order fulfillment process
    /// </summary>
    [HttpPost("fulfill")]
    public async Task<IActionResult> FulfillOrder([FromBody] FulfillmentRequest request)
    {
        var parameters = new OrderParameters
        {
            OrderId = request.OrderId,
            ShippingAddress = request.ShippingAddress
        };

        var operationRequest = new OperationRequest<OrderParameters>
        {
            OperationType = "fulfillment",
            Parameters = parameters
        };

        return await StartOperation(operationRequest);
    }
}

// Request models for order operations
public class CreateOrderRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public ShippingAddress? ShippingAddress { get; set; }
}

public class PaymentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentMethodId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class FulfillmentRequest
{
    public string OrderId { get; set; } = string.Empty;
    public ShippingAddress? ShippingAddress { get; set; }
}
