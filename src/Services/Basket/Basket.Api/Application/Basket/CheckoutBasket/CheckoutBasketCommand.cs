namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketCommand
{
    // Basket
    public string UserName { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public decimal TotalPrice { get; set; } = 0.0m;

    // Shipping and BillingAddress
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? AddressLine { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Coordinates { get; set; }

    // Payment
    public string? CardName { get; set; }
    public string? CardNumber { get; set; }
    public string? Expiration { get; set; }
    public string? Cvv { get; set; }
    public int PaymentMethod { get; set; } = 0;
    public string? RequestId { get; set; }
}