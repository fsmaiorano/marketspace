using Order.Api.Domain.Entities;
using Order.Api.Domain.ValueObjects;
using Order.Api.Endpoints.Dto;

namespace Order.Api.Application.Extensions;

public static class OrderExtensions
{
    public static Address ToAddress(this AddressDto addressDto)
    {
        return Address.Of(
            firstName: addressDto.FirstName,
            lastName: addressDto.LastName,
            emailAddress: addressDto.EmailAddress,
            addressLine: addressDto.AddressLine,
            country: addressDto.Country,
            state: addressDto.State,
            zipCode: addressDto.ZipCode,
            coordinates: addressDto.Coordinates
        );
    }

    public static Payment ToPayment(this PaymentDto paymentDto)
    {
        return Payment.Of(
            cardNumber: paymentDto.CardNumber,
            cardName: paymentDto.CardName,
            expiration: paymentDto.Expiration,
            cvv: paymentDto.Cvv,
            paymentMethod: paymentDto.PaymentMethod
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

    public static List<OrderItemEntity> ToOrderItems(this List<OrderItemDto> items, OrderId orderId)
    {
        return items.Select(item => orderId.ToOrderItemEntity(item)).ToList();
    }
}