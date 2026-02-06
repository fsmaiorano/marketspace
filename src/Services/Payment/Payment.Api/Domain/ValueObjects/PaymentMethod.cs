using System.Text.Json.Serialization;
using Payment.Api.Domain.Enums;

namespace Payment.Api.Domain.ValueObjects;

public record PaymentMethod
{
    public string Value { get; set; } = string.Empty;

    private PaymentMethod(string value) => Value = value;

    [JsonConstructor]
    public PaymentMethod() { }

    public static PaymentMethod Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Payment method cannot be empty.", nameof(value));

        return !Enum.TryParse(typeof(PaymentMethodEnum), value, true, out _)
            ? throw new ArgumentException($"Invalid payment method: {value}.", nameof(value))
            : new PaymentMethod(value);
    }
}