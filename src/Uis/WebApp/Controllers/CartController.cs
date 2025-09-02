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

        CreateOrUpdateBasketRequest createBasketRequest = new() { Username = "mock", Items = [] };

        GetBasketResponse response = await service.GetBasketByUsernameAsync("fsmaiorano");
        return Json(response);
    }
}