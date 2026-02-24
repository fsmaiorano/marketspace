using Basket.Api.Domain.Entities;
using Basket.Api.Domain.Repositories;
using Basket.Api.Endpoints.Dto;
using BuildingBlocks;
using BuildingBlocks.Loggers;
using ShoppingCartEntity = Basket.Api.Domain.Entities.ShoppingCartEntity;

namespace Basket.Api.Application.Basket.CreateBasket;

public record CreateBasketCommand(
    string Username,
    IReadOnlyList<ShoppingCartItemDto> Items
);

public record CreateBasketResult(ShoppingCartEntity ShoppingCart);

public sealed class CreateBasket(
    IBasketDataRepository dataRepository,
    IAppLogger<CreateBasket> logger)
{
    public async Task<Result<CreateBasketResult>> HandleAsync(CreateBasketCommand command)
    {
        try
        {
            logger.LogInformation(LogTypeEnum.Application, "Processing create basket request for user: {Username}",
                command.Username);

            ShoppingCartEntity cartEntity = new()
            {
                Username = command.Username,
                Items = command.Items.Select(item => new ShoppingCartItemEntity
                {
                    ProductName = item.ProductName,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                }).ToList(),
            };

            ShoppingCartEntity shoppingCart = await dataRepository.CreateCartAsync(cartEntity);

            logger.LogInformation(LogTypeEnum.Business,
                "Basket created successfully. Username: {Username}, ItemCount: {ItemCount}",
                cartEntity.Username,
                cartEntity.Items.Count);
         
            return Result<CreateBasketResult>.Success(new CreateBasketResult(shoppingCart));
        }
        catch (Exception ex)
        {
            logger.LogError(LogTypeEnum.Exception, ex, "An error occurred while creating the basket: {Command}",
                command);
            return Result<CreateBasketResult>.Failure($"An error occurred while creating the basket: {ex.Message}");
        }
    }
}