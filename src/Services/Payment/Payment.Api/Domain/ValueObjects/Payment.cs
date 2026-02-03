namespace Payment.Api.Domain.ValueObjects;

public record Payment
{
    public string CardNumber { get; init; } = null!;
    public string CardName { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int PaymentMethod { get; init; } = 0!;

    private Payment(string cardNumber, string cardName, string expiration, string cvv, int paymentMethod)
    {
        CardNumber = cardNumber;
        CardName = cardName;
        Expiration = expiration;
        Cvv = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Payment Create(string cardNumber, string cardName, string expiration, string cvv, int paymentMethod)
    {
        return Of(cardNumber, cardName, expiration, cvv, paymentMethod);
    }

    public static Payment Of(string cardNumber, string cardName, string expiration, string cvv, int paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber, "Card number cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName, "Card name cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(expiration, "Expiration date cannot be empty.");
        ArgumentOutOfRangeException.ThrowIfGreaterThan(cvv.Length, 3, "CVV must be 3 digits long.");

        return new Payment(cardNumber, cardName, expiration, cvv, paymentMethod);
    }

    public static Payment FromString(string paymentString)
    {
        if (string.IsNullOrWhiteSpace(paymentString))
            throw new ArgumentException("Payment string cannot be empty.", nameof(paymentString));

        string[] parts = paymentString.Split('|');
        if (parts.Length != 5)
            throw new ArgumentException("Invalid payment string format.", nameof(paymentString));

        if (!int.TryParse(parts[4], out int paymentMethod))
            throw new ArgumentException("Invalid payment method format.", nameof(paymentString));

        return new Payment(parts[0], parts[1], parts[2], parts[3], paymentMethod);
    }

    public override string ToString()
    {
        return $"{CardNumber}|{CardName}|{Expiration}|{Cvv}|{PaymentMethod}";
    }
}