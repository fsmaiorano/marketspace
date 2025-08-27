using Microsoft.AspNetCore.Mvc;
using WebApp.Services;

namespace WebApp.Controllers;

public class CatalogController(IMarketSpaceService service, ILogger<CatalogController> logger) : Controller
{ 
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            logger.LogInformation("Fetching products from catalog service");
            var products = await service.GetProductsAsync();
            return Json(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching products");
            return StatusCode(500, "Internal server error");
        }
    }
}