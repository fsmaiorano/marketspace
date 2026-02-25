namespace Order.Api.Domain.Entities;

public class PaymentSummaryEntity
{
    public string CardName { get; init; } = string.Empty;
    public string MaskedCardNumber { get; init; } = string.Empty;
    public int PaymentMethod { get; init; } = 0;
}
