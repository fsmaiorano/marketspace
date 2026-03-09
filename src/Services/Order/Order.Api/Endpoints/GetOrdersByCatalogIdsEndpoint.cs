using BuildingBlocks;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Application.Order.GetOrdersByCatalogIds;
using Order.Api.Endpoints.Dto;

namespace Order.Api.Endpoints;

public static class GetOrdersByCatalogIdsEndpoint
{
    public sealed class GetOrdersByCatalogIdsRequest
    {
        public List<Guid> CatalogIds { get; init; } = [];
        public int Limit { get; init; } = 50;
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/order/catalog-items",
                async ([FromBody] GetOrdersByCatalogIdsRequest request, [FromServices] GetOrdersByCatalogIds handler) =>
                {
                    GetOrdersByCatalogIdsQuery query = new(request.CatalogIds, request.Limit);
                    Result<GetOrdersByCatalogIdsResult> result = await handler.HandleAsync(query);

                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(new
                        {
                            orders = result.Data.Orders.Select(order => new OrderDto
                            {
                                Id = order.Id.Value,
                                CustomerId = order.CustomerId.Value,
                                ShippingAddress = new AddressDto
                                {
                                    FirstName = order.ShippingAddress.FirstName,
                                    LastName = order.ShippingAddress.LastName,
                                    EmailAddress = order.ShippingAddress.EmailAddress,
                                    AddressLine = order.ShippingAddress.AddressLine,
                                    Country = order.ShippingAddress.Country,
                                    State = order.ShippingAddress.State,
                                    ZipCode = order.ShippingAddress.ZipCode,
                                    Coordinates = order.ShippingAddress.Coordinates
                                },
                                BillingAddress = new AddressDto
                                {
                                    FirstName = order.BillingAddress.FirstName,
                                    LastName = order.BillingAddress.LastName,
                                    EmailAddress = order.BillingAddress.EmailAddress,
                                    AddressLine = order.BillingAddress.AddressLine,
                                    Country = order.BillingAddress.Country,
                                    State = order.BillingAddress.State,
                                    ZipCode = order.BillingAddress.ZipCode,
                                    Coordinates = order.BillingAddress.Coordinates
                                },
                                Payment = new PaymentDto
                                {
                                    CardName = order.Payment.CardName,
                                    CardNumber = order.Payment.CardNumber,
                                    Expiration = order.Payment.Expiration,
                                    Cvv = order.Payment.Cvv,
                                    PaymentMethod = order.Payment.PaymentMethod
                                },
                                Status = order.Status.ToString(),
                                Items = order.Items.Select(item => new OrderItemDto
                                {
                                    OrderId = item.OrderId.Value,
                                    CatalogId = item.CatalogId.Value,
                                    Quantity = item.Quantity,
                                    Price = item.Price.Value
                                }).ToList(),
                                TotalAmount = order.TotalAmount.Value,
                                CreatedAt = order.CreatedAt!.Value
                            }).ToList()
                        })
                        : Results.BadRequest(result.Error);
                })
            .WithName("GetOrdersByCatalogIds")
            .WithTags("Order")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
