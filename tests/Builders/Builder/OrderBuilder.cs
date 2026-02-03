using Bogus;
using Order.Api.Application.Dto;
using Order.Api.Application.Order.CreateOrder;
using Order.Api.Application.Order.DeleteOrder;
using Order.Api.Application.Order.UpdateOrder;
using Order.Api.Domain.Entities;
using Order.Api.Domain.Enums;
using Order.Api.Domain.ValueObjects;
using OrderPayment = Order.Api.Domain.ValueObjects.Payment;

namespace Builder;

public static class OrderBuilder
{
    private static readonly OrderStatusEnum[] OrderStatuses =
    {
        OrderStatusEnum.Created, OrderStatusEnum.Processing, OrderStatusEnum.Completed,
        OrderStatusEnum.ReadyForDelivery, OrderStatusEnum.DeliveryInProgress, OrderStatusEnum.Delivered,
        OrderStatusEnum.Finalized, OrderStatusEnum.Cancelled, OrderStatusEnum.CancelledByCustomer
    };

    private static readonly PaymentMethodEnum[] PaymentMethods =
    {
        PaymentMethodEnum.Cash, PaymentMethodEnum.CreditCard, PaymentMethodEnum.DebitCard
    };

    public static Faker<OrderEntity> CreateOrderFaker(Guid customerId = default)
    {
        return new Faker<OrderEntity>()
            .CustomInstantiator(f => OrderEntity.Create(
                orderId: OrderId.Of(f.Random.Guid()),
                customerId: customerId == Guid.Empty ? CustomerId.Of(f.Random.Guid()) : CustomerId.Of(customerId),
                shippingAddress: CreateAddressFaker().Generate(),
                billingAddress: CreateAddressFaker().Generate(),
                payment: CreatePaymentFaker().Generate(),
                items: CreateOrderItemFaker().Generate(f.Random.Int(1, 5))
            ));
    }

    public static Faker<OrderItemEntity> CreateOrderItemFaker(OrderId? orderId = null)
    {
        return new Faker<OrderItemEntity>()
            .CustomInstantiator(f => OrderItemEntity.Create(
                orderId: orderId ?? OrderId.Of(f.Random.Guid()),
                catalogId: CatalogId.Of(f.Random.Guid()),
                quantity: f.Random.Int(1, 10),
                price: Price.Of(f.Finance.Amount(1, 1000))
            ));
    }

    public static Faker<Address> CreateAddressFaker()
    {
        return new Faker<Address>()
            .CustomInstantiator(f => Address.Of(
                firstName: f.Name.FirstName(),
                lastName: f.Name.LastName(),
                emailAddress: f.Internet.Email(),
                addressLine: f.Address.StreetAddress(),
                country: f.Address.Country(),
                state: f.Address.State(),
                zipCode: f.Address.ZipCode(),
                coordinates: $"{f.Address.Latitude()},{f.Address.Longitude()}"
            ));
    }

    public static Faker<OrderPayment> CreatePaymentFaker()
    {
        return new Faker<OrderPayment>()
            .CustomInstantiator(f => OrderPayment.Of(
                cardNumber: f.Finance.CreditCardNumber(),
                cardName: f.Name.FullName(),
                expiration: f.Date.Future().ToString("MM/yy"),
                cvv: f.Finance.Random.Number(100, 999).ToString(),
                paymentMethod: (int)f.PickRandom(PaymentMethods)
            ));
    }

    public static Faker<AddressDto> CreateAddressDtoFaker()
    {
        return new Faker<AddressDto>()
            .RuleFor(a => a.FirstName, f => f.Name.FirstName())
            .RuleFor(a => a.LastName, f => f.Name.LastName())
            .RuleFor(a => a.EmailAddress, f => f.Internet.Email())
            .RuleFor(a => a.AddressLine, f => f.Address.StreetAddress())
            .RuleFor(a => a.Country, f => f.Address.Country())
            .RuleFor(a => a.State, f => f.Address.State())
            .RuleFor(a => a.Coordinates, f => $"{f.Address.Latitude()},{f.Address.Longitude()}")
            .RuleFor(a => a.ZipCode, f => f.Address.ZipCode());
    }

    public static Faker<PaymentDto> CreatePaymentDtoFaker()
    {
        return new Faker<PaymentDto>()
            .RuleFor(p => p.CardNumber, f => f.Finance.CreditCardNumber())
            .RuleFor(p => p.CardName, f => f.Name.FullName())
            .RuleFor(p => p.Expiration, f => f.Date.Future().ToString("MM/yy"))
            .RuleFor(p => p.Cvv, f => f.Finance.Random.Number(100, 999).ToString())
            .RuleFor(p => p.PaymentMethod, f => (int)f.PickRandom(PaymentMethods));
    }

    public static Faker<OrderItemDto> CreateOrderItemDtoFaker()
    {
        return new Faker<OrderItemDto>()
            .RuleFor(o => o.CatalogId, f => f.Random.Guid())
            .RuleFor(o => o.Quantity, f => f.Random.Int(1, 10))
            .RuleFor(o => o.Price, f => f.Finance.Amount(1, 1000, 2));
    }

    public static Faker<List<OrderItemDto>> CreateOrderItemDtoFaker(int count)
    {
        return new Faker<List<OrderItemDto>>()
            .CustomInstantiator(f => CreateOrderItemDtoFaker().Generate(count));
    }

    public static Faker<CreateOrderCommand> CreateCreateOrderCommandFaker(Guid? customerId = null)
    {
        return new Faker<CreateOrderCommand>()
            .RuleFor(c => c.CustomerId, f => customerId ?? f.Random.Guid())
            .RuleFor(c => c.ShippingAddress, f => CreateAddressDtoFaker().Generate())
            .RuleFor(c => c.BillingAddress, f => CreateAddressDtoFaker().Generate())
            .RuleFor(c => c.Payment, f => CreatePaymentDtoFaker().Generate())
            .RuleFor(c => c.Status, f => f.PickRandom(OrderStatuses))
            .RuleFor(c => c.Items, f => CreateOrderItemDtoFaker().Generate(f.Random.Int(1, 5)))
            .RuleFor(c => c.TotalAmount, f => f.Finance.Amount(1, 5000, 2));
    }

    public static Faker<UpdateOrderCommand> CreateUpdateOrderCommandFaker(Guid? orderId = null)
    {
        return new Faker<UpdateOrderCommand>()
            .RuleFor(m => m.Id, f => orderId ?? f.Random.Guid())
            .RuleFor(m => m.CustomerId, f => f.Random.Guid())
            .RuleFor(m => m.ShippingAddress, f => CreateAddressDtoFaker().Generate())
            .RuleFor(m => m.BillingAddress, f => CreateAddressDtoFaker().Generate())
            .RuleFor(m => m.Payment, f => CreatePaymentDtoFaker().Generate())
            .RuleFor(m => m.Status, f => f.PickRandom(OrderStatuses))
            .RuleFor(m => m.Items, f => CreateOrderItemDtoFaker().Generate(f.Random.Int(1, 5)))
            .RuleFor(c => c.TotalAmount, f => f.Finance.Amount(1, 5000, 2));
    }

    public static Faker<DeleteOrderCommand> CreateDeleteOrderCommandFaker(Guid? orderId = null)
    {
        return new Faker<DeleteOrderCommand>()
            .RuleFor(m => m.Id, f => orderId ?? f.Random.Guid());
    }

    public static OrderEntity GenerateOrder(Guid customerId = default) =>
        CreateOrderFaker(customerId).Generate();

    public static OrderItemEntity GenerateOrderItem(OrderId? orderId = null) =>
        CreateOrderItemFaker(orderId).Generate();

    public static List<OrderItemEntity> GenerateOrderItems(OrderId orderId, int count = 3) =>
        CreateOrderItemFaker(orderId).Generate(count);

    public static Address GenerateAddress() =>
        CreateAddressFaker().Generate();

    public static OrderPayment GeneratePayment() =>
        CreatePaymentFaker().Generate();

    public static AddressDto GenerateAddressDto() =>
        CreateAddressDtoFaker().Generate();

    public static PaymentDto GeneratePaymentDto() =>
        CreatePaymentDtoFaker().Generate();

    public static OrderItemDto GenerateOrderItemDto() =>
        CreateOrderItemDtoFaker().Generate();

    public static CreateOrderCommand GenerateCreateCommand(Guid? customerId = null) =>
        CreateCreateOrderCommandFaker(customerId).Generate();

    public static UpdateOrderCommand GenerateUpdateCommand(Guid? orderId = null) =>
        CreateUpdateOrderCommandFaker(orderId).Generate();

    public static DeleteOrderCommand GenerateDeleteCommand(Guid? orderId = null) =>
        CreateDeleteOrderCommandFaker(orderId).Generate();

    public static List<OrderEntity> GenerateOrders(int count = 5, Guid customerId = default) =>
        CreateOrderFaker(customerId).Generate(count);

    public static List<CreateOrderCommand> GenerateCreateCommands(int count = 5, Guid? customerId = null) =>
        CreateCreateOrderCommandFaker(customerId).Generate(count);

    public static List<AddressDto> GenerateAddressDtos(int count = 5) =>
        CreateAddressDtoFaker().Generate(count);

    public static List<PaymentDto> GeneratePaymentDtos(int count = 5) =>
        CreatePaymentDtoFaker().Generate(count);

    public static List<OrderItemDto> GenerateOrderItemDtos(int count = 5) =>
        CreateOrderItemDtoFaker().Generate(count);
}