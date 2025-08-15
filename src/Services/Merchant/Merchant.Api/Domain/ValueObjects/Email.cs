using BuildingBlocks.Exceptions;

namespace Merchant.Api.Domain.ValueObjects;

public record Email
{
    public string Value { get; init; }

    private Email(string value) => Value = value;

    public static Email Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new DomainException("Invalid email format.");

        return new Email(value);
    }
}