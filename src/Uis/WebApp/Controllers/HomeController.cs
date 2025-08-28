using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Dtos;
using WebApp.Services;

namespace WebApp.Controllers;

public class HomeController(IMarketSpaceService service, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index()
    {
        try
        {
            logger.LogInformation("Fetching initial products for home page");
            GetCatalogResponse products = await service.GetProductsAsync(1, 20);
            return View(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while fetching products for home page");
            GetCatalogResponse emptyResponse = new GetCatalogResponse
            {
                Products = [], Count = 0, PageIndex = 1, PageSize = 20
            };
            return View(emptyResponse);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}