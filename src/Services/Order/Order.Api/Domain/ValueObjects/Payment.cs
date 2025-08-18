namespace Order.Api.Domain.ValueObjects;

public record Payment
{
    public string CardNumber { get; init; } = null!;
    public string CardName { get; init; } = null!;
    public string Expiration { get; init; } = null!;
    public string Cvv { get; init; } = null!;
    public int PaymentMethod { get; set; } = 0!;

    protected Payment()
    {
    }

    private Payment(string cardNumber, string cardName, string expiration, string cvv, int paymentMethod)
    {
        CardNumber = cardNumber;
        CardName = cardName;
        Expiration = expiration;
        Cvv = cvv;
        PaymentMethod = paymentMethod;
    }

    public static Payment Of(string cardNumber, string cardName, string expiration, string cvv, int paymentMethod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cardNumber, "Card number cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(cardName, "Card name cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(expiration, "Expiration date cannot be empty.");
        ArgumentOutOfRangeException.ThrowIfGreaterThan(cvv.Length, 3, "CVV must be 3 digits long.");

        return new Payment(cardNumber, cardName, expiration, cvv, paymentMethod);
    }
}