using BuildingBlocks;
using Merchant.Api.Application.Merchant.GetMerchantByUserId;
using Merchant.Api.Endpoints.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Merchant.Api.Endpoints;

public static class GetMerchantByUserIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/merchant/by-user/{userId:guid}",
                async ([FromRoute] Guid userId, [FromServices] GetMerchantByUserId handler) =>
                {
                    GetMerchantByUserIdQuery query = new(userId);
                    Result<GetMerchantByUserIdResult> result = await handler.HandleAsync(query);
                    return result is { IsSuccess: true, Data: not null }
                        ? Results.Ok(new MerchantDto
                        {
                            Id = result.Data.Merchant.Id.Value,
                            Name = result.Data.Merchant.Name,
                            Description = result.Data.Merchant.Description,
                            Email = result.Data.Merchant.Email.Value,
                            PhoneNumber = result.Data.Merchant.PhoneNumber,
                            Address = result.Data.Merchant.Address,
                            CreatedAt = result.Data.Merchant.CreatedAt,
                            UpdatedAt = result.Data.Merchant.UpdatedAt
                        })
                        : Results.NotFound(result.Error);
                })
            .WithName("GetMerchantByUserId")
            .WithTags("Merchant")
            .Produces<MerchantDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);
    }
}
