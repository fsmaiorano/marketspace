namespace Basket.Api.Application.Basket.CheckoutBasket.Dtos;

/// <summary>
/// DTO containing all data needed for basket checkout
/// </summary>
public class CheckoutDataDto
{
    public Guid CustomerId { get; init; }
    public string UserName { get; init; } = null!;
    public CheckoutAddressDto Address { get; init; } = null!;
    public CheckoutPaymentDto Payment { get; init; } = null!;
    public string? CorrelationId { get; init; }
    public string? IdempotencyKey { get; init; }
}

public class CheckoutAddressDto
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string EmailAddress { get; init; } = null!;
    public string AddressLine { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string State { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
}

public class CheckoutPaymentDto
{
    public string CardName { get; init; } = null!;
    public string CardNumber { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int PaymentMethod { get; init; }
}
