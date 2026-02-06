using System.Text.Json.Serialization;

namespace Order.Api.Domain.ValueObjects;

public record Address
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string EmailAddress { get; private set; } = null!;
    public string AddressLine { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string ZipCode { get; private set; } = null!;
    public string Coordinates { get; private set; } = null!;

    protected Address()
    {
    }

    [JsonConstructor]
    public Address(string firstName, string lastName, string emailAddress, string addressLine, string country,
        string state, string zipCode, string coordinates)
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

    public static Address Of(string firstName, string lastName, string emailAddress, string addressLine,
        string country, string state, string zipCode, string coordinates)
    {
        ValidateInput(firstName, nameof(firstName));
        ValidateInput(emailAddress, nameof(emailAddress));
        ValidateInput(addressLine, nameof(addressLine));
        ValidateInput(country, nameof(country));
        ValidateInput(state, nameof(state));
        ValidateInput(zipCode, nameof(zipCode));
        ValidateInput(coordinates, nameof(coordinates));

        return new Address(firstName, lastName, emailAddress, addressLine, country, state, zipCode, coordinates);
    }

    public static Address FromString(string addressString)
    {
        if (string.IsNullOrWhiteSpace(addressString))
            throw new ArgumentException("Address string cannot be empty.", nameof(addressString));

        string[] parts = addressString.Split('|');
        return parts.Length != 8
            ? throw new ArgumentException("Invalid address string format.", nameof(addressString))
            : new Address(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5], parts[6], parts[7]);
    }

    public override string ToString()
    {
        return $"{FirstName}|{LastName}|{EmailAddress}|{AddressLine}|{Country}|{State}|{ZipCode}|{Coordinates}";
    }

    private static void ValidateInput(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be empty.", parameterName);
    }
}