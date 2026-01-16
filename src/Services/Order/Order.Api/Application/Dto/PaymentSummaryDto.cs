namespace Order.Api.Application.Dto;

public class PaymentSummaryDto
{
    public string CardName { get; init; } = string.Empty;
    public string MaskedCardNumber { get; init; } = string.Empty;
    public int PaymentMethod { get; init; } = 0;
}
