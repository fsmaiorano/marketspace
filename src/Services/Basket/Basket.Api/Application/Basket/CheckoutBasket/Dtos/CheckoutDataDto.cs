namespace Basket.Api.Application.Basket.CheckoutBasket.Dtos;

/// <summary>
/// DTO containing all data needed for basket checkout
/// </summary>
public class CheckoutDataDto
{
    public Guid CustomerId { get; init; }
    public string UserName { get; init; } = null!;
    public CheckoutAddressDto? Address { get; init; }
    public CheckoutPaymentDto? Payment { get; init; }
    public string? CorrelationId { get; init; }
}

public class CheckoutAddressDto
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? EmailAddress { get; init; }
    public string? AddressLine { get; init; }
    public string? Country { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? Coordinates { get; set; }
}

public class CheckoutPaymentDto
{
    public string? CardName { get; init; }
    public string? CardNumber { get; init; }
    public string? Expiration { get; init; }
    public string? Cvv { get; init; }
    public int PaymentMethod { get; init; }
}
