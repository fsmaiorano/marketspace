namespace Order.Api.Application.Mapping;

public static class PaymentStatusMapper
{
    /// <summary>
    /// Maps Payment status to Order status
    /// </summary>
    /// <param name="paymentStatus">Payment status from Payment service</param>
    /// <returns>Corresponding Order status</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when payment status is unknown</exception>
    public static Domain.Enums.OrderStatusEnum ToOrderStatus(int paymentStatus)
    {
        return paymentStatus switch
        {
            1 => Domain.Enums.OrderStatusEnum.Created,           // Created -> Created
            2 => Domain.Enums.OrderStatusEnum.Processing,        // Processing -> Processing
            3 => Domain.Enums.OrderStatusEnum.Processing,        // Authorized -> Processing
            4 => Domain.Enums.OrderStatusEnum.Completed,         // Captured -> Completed
            5 => Domain.Enums.OrderStatusEnum.Cancelled,         // Failed -> Cancelled
            6 => Domain.Enums.OrderStatusEnum.Cancelled,         // Rejected -> Cancelled
            7 => Domain.Enums.OrderStatusEnum.Cancelled,         // Cancelled -> Cancelled
            8 => Domain.Enums.OrderStatusEnum.CancelledByCustomer, // Refunded -> CancelledByCustomer
            9 => Domain.Enums.OrderStatusEnum.CancelledByCustomer, // Chargeback -> CancelledByCustomer
            _ => throw new ArgumentOutOfRangeException(nameof(paymentStatus), paymentStatus, "Unknown payment status")
        };
    }

    /// <summary>
    /// Maps Order status to Payment status (reverse mapping)
    /// </summary>
    /// <param name="orderStatus">Order status from Order service</param>
    /// <returns>Corresponding Payment status or null if no direct mapping exists</returns>
    public static int? ToPaymentStatus(Domain.Enums.OrderStatusEnum orderStatus)
    {
        return orderStatus switch
        {
            Domain.Enums.OrderStatusEnum.Created => 1,           // Created -> Created
            Domain.Enums.OrderStatusEnum.Processing => 2,        // Processing -> Processing
            Domain.Enums.OrderStatusEnum.Completed => 4,         // Completed -> Captured
            Domain.Enums.OrderStatusEnum.Cancelled => 7,         // Cancelled -> Cancelled
            Domain.Enums.OrderStatusEnum.CancelledByCustomer => 8, // CancelledByCustomer -> Refunded
            // Order statuses that don't have payment equivalents
            Domain.Enums.OrderStatusEnum.ReadyForDelivery => null,
            Domain.Enums.OrderStatusEnum.DeliveryInProgress => null,
            Domain.Enums.OrderStatusEnum.Delivered => null,
            Domain.Enums.OrderStatusEnum.Finalized => null,
            _ => throw new ArgumentOutOfRangeException(nameof(orderStatus), orderStatus, "Unknown order status")
        };
    }
}
