using Bogus;
using Payment.Api.Application.Payment.CreatePayment;
using Payment.Api.Application.Payment.UpdatePayment;
using Payment.Api.Domain.Entities;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.ValueObjects;

namespace Builder;

public static class PaymentBuilder
{
    private static readonly string[] Currencies = { "USD", "EUR", "BRL" };
    private static readonly string[] Methods = { "CreditCard", "DebitCard", "Cash" };
    private static readonly string[] Providers = { "Stripe", "PayPal", "Adyen" };
    
    private static readonly PaymentStatusEnum[] PaymentStatuses =
    {
        PaymentStatusEnum.Created, PaymentStatusEnum.Processing, PaymentStatusEnum.Authorized,
        PaymentStatusEnum.Failed, PaymentStatusEnum.Captured, PaymentStatusEnum.Refunded
    };

    public static Faker<PaymentEntity> CreatePaymentFaker(Guid orderId = default)
    {
        return new Faker<PaymentEntity>()
            .CustomInstantiator(f => PaymentEntity.Create(
                orderId: orderId == Guid.Empty ? f.Random.Guid() : orderId,
                amount: f.Finance.Amount(1, 1000),
                currency: f.PickRandom(Currencies),
                method: PaymentMethod.Of(f.PickRandom(Methods)),
                provider: f.PickRandom(Providers)
            ));
    }

    public static Faker<CreatePaymentCommand> CreateCreatePaymentCommandFaker(Guid orderId = default)
    {
        return new Faker<CreatePaymentCommand>()
            .RuleFor(c => c.OrderId, f => orderId == Guid.Empty ? f.Random.Guid() : orderId)
            .RuleFor(c => c.Amount, f => f.Finance.Amount(1, 1000))
            .RuleFor(c => c.Currency, f => f.PickRandom(Currencies))
            .RuleFor(c => c.Method, f => f.PickRandom(Methods))
            .RuleFor(c => c.Provider, f => f.PickRandom(Providers));
    }

    public static Faker<UpdatePaymentCommand> CreateUpdatePaymentCommandFaker(Guid id)
    {
        return new Faker<UpdatePaymentCommand>()
            .RuleFor(c => c.Id, id)
            .RuleFor(c => c.Status, f => f.PickRandom(PaymentStatuses))
            .RuleFor(c => c.StatusDetail, f => f.Lorem.Sentence())
            .RuleFor(c => c.ProviderTransactionId, f => f.Random.AlphaNumeric(10))
            .RuleFor(c => c.AuthorizationCode, f => f.Random.AlphaNumeric(6));
    }
}
