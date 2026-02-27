using System.Text.Json.Serialization;

namespace Basket.Api.Domain.ValueObjects;

public class CheckoutData
{
    public Guid CustomerId { get; private set; }
    public string UserName { get; private set; }
    public CheckoutAddress? Address { get; private set; }
    public CheckoutPayment? Payment { get; private set; }
    public string? CorrelationId { get; private set; }

    [JsonConstructor]
    public CheckoutData(Guid customerId, string userName, CheckoutAddress? address, CheckoutPayment? payment,
        string? correlationId)
    {
        CustomerId = customerId;
        UserName = userName;
        Address = address;
        Payment = payment;
        CorrelationId = correlationId;
    }

    public static CheckoutData Create(
        Guid customerId,
        string userName,
        CheckoutAddress? address = null,
        CheckoutPayment? payment = null,
        string? correlationId = null)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("CustomerId cannot be empty.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("UserName is required.", nameof(userName));

        return new CheckoutData(customerId, userName, address, payment, correlationId);
    }
}

/// <summary>
/// Value Object representing checkout address
/// </summary>
public class CheckoutAddress
{
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? EmailAddress { get; private set; }
    public string? AddressLine { get; private set; }
    public string? Country { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Coordinates { get; private set; }

    private CheckoutAddress() { } // For EF Core

    [JsonConstructor]
    public CheckoutAddress(string? firstName, string? lastName, string? emailAddress, string? addressLine,
        string? country, string? state, string? zipCode, string? coordinates)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = emailAddress;
        AddressLine = addressLine;
        Country = country;
        State = state;
        ZipCode = zipCode;
        Coordinates = coordinates;
    }

    public static CheckoutAddress Create(
        string? firstName,
        string? lastName,
        string? emailAddress,
        string? addressLine,
        string? country,
        string? state,
        string? zipCode,
        string? coordinates = null)
    {
        return new CheckoutAddress(firstName, lastName, emailAddress, addressLine, country, state, zipCode,
            coordinates);
    }
}

/// <summary>
/// Value Object representing checkout payment
/// </summary>
public class CheckoutPayment
{
    public string? CardName { get; private set; }
    public string? CardNumber { get; private set; }
    public string? Expiration { get; private set; }
    public string? Cvv { get; private set; }
    public int PaymentMethod { get; private set; }

    private CheckoutPayment() { } // For EF Core

    [JsonConstructor]
    public CheckoutPayment(string? cardName, string? cardNumber, string? expiration, string? cvv, int paymentMethod)
    {
        CardName = cardName;
        CardNumber = cardNumber;
        Expiration = expiration;
        Cvv = cvv;
        PaymentMethod = paymentMethod;
    }

    public static CheckoutPayment Create(
        string? cardName,
        string? cardNumber,
        string? expiration,
        string? cvv,
        int paymentMethod)
    {
        return new CheckoutPayment(cardName, cardNumber, expiration, cvv, paymentMethod);
    }
}