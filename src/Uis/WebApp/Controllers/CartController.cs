using Microsoft.AspNetCore.Mvc;
using WebApp.Dtos;
using WebApp.Services;

namespace WebApp.Controllers;

[Route("api/[controller]")]
public class CartController(IMarketSpaceService service, ILogger<CartController> logger) : Controller
{
    [HttpPost("CartHandler")]
    public IActionResult CartHandler(string productId)
    {
        logger.LogInformation("Adding product {ProductId} to cart", productId);

        CreateOrUpdateBasketRequest createBasketRequest = new()
        {
            Username = "mock", Items = []
        };

        var response = new { Success = true, Message = "Product added to cart", ProductId = productId };

        return Json(response);
    }
}