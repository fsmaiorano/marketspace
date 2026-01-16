namespace Basket.Api.Application.Basket.CheckoutBasket;

public class CheckoutBasketCommand
{
    // Basket
    public string UserName { get; set; } = null!;
    public Guid CustomerId { get; set; } = Guid.CreateVersion7();
    public decimal TotalPrice { get; set; } = 0.0m;

    // Shipping and BillingAddress
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string EmailAddress { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public string Country { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;

    // Payment
    public string CardName { get; set; } = null!;
    public string CardNumber { get; set; } = null!;
    public string Expiration { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public int PaymentMethod { get; set; } = 0;
    public string? RequestId { get; set; }
    public string? IdempotencyKey { get; set; }
}