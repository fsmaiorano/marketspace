namespace Order.Api.Domain.Enums;

public enum OrderStatusEnum
{
    Created = 1,
    Processing = 2,
    Completed = 3,
    ReadyForDelivery = 4,
    DeliveryInProgress = 5,
    Delivered = 6,
    Finalized = 7,
    Cancelled = 90,
    CancelledByCustomer = 91,
}