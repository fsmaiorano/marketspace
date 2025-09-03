using Microsoft.AspNetCore.Mvc;
using WebApp.Dtos;
using WebApp.Services;

namespace WebApp.Controllers;

[Route("api/[controller]")]
public class CartController(IMarketSpaceService service, ILogger<CartController> logger) : Controller
{
    [HttpPost("CartHandler")]
    public async Task<IActionResult> CartHandler(string productId)
    {
        logger.LogInformation("Adding product {ProductId} to cart", productId);
        GetBasketResponse? storedCart = await service.GetBasketByUsernameAsync("fsmaiorano");
        return Json(storedCart);
    }

    [HttpGet("GetCart")]
    public async Task<IActionResult> GetCart()
    {
        logger.LogInformation("Getting cart for user");
        GetBasketResponse? storedCart = await service.GetBasketByUsernameAsync("fsmaiorano");
        return Json(storedCart);
    }
}