using Order.Api.Application.Dto;
using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;

namespace Order.Api.Application.Extensions;

public static class OrderExtensions
{
    public static Address ToAddress(this AddressDto addressDto)
    {
        return Address.Of(
            firstName: addressDto.FirstName,
            lastName: addressDto.LastName,
            state: addressDto.State,
            country: addressDto.Country,
            zipCode: addressDto.ZipCode,
            emailAddress: addressDto.EmailAddress,
            addressLine: addressDto.AddressLine
        );
    }

    public static Payment ToPayment(this PaymentDto paymentDto)
    {
        return Payment.Of(
            cardNumber: paymentDto.CardNumber,
            cardName: paymentDto.CardName,
            expiration: paymentDto.Expiration,
            paymentMethod: paymentDto.PaymentMethod,
            cvv: paymentDto.Cvv
        );
    }

    public static OrderItemEntity ToOrderItemEntity(this OrderId orderId, OrderItemDto orderItemDto)
    {
        return OrderItemEntity.Create(
            orderId: orderId,
            catalogId: CatalogId.Of(orderItemDto.CatalogId),
            quantity: orderItemDto.Quantity,
            price: Price.Of(orderItemDto.Price)
        );
    }
}