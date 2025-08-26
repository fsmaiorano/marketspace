using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using System.Text.Json;

namespace WebApp.Controllers;

public class HomeController : Controller
{
    private readonly CatalogSSEService _catalogService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(CatalogSSEService catalogService, ILogger<HomeController> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Legacy catalog endpoint for backward compatibility with existing demo
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Catalog([FromBody] CatalogRequest request)
    {
        try
        {
            var parameters = new CatalogParameters
            {
                Page = request.Parameters?.GetProperty("page").GetInt32() ?? 1,
                PageSize = request.Parameters?.GetProperty("pageSize").GetInt32() ?? 50,
                IncludeOutOfStock = request.Parameters?.GetProperty("includeOutOfStock").GetBoolean() ?? true,
                Query = request.Parameters?.GetProperty("query").GetString(),
                Category = request.Parameters?.GetProperty("category").GetString(),
                InStockOnly = request.Parameters?.GetProperty("inStockOnly").GetBoolean() ?? false
            };

            var operationType = request.OperationType ?? "catalog";
            var operationId = await _catalogService.StartOperationAsync(operationType, parameters);

            return Json(new
            {
                success = true,
                operationId,
                message = "Operation started successfully",
                sseUrl = Url.Action("StreamStatus", new { operationId })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start catalog operation");
            return Json(new { success = false, message = "Failed to start operation" });
        }
    }

    /// <summary>
    /// Legacy SSE endpoint for backward compatibility
    /// </summary>
    [HttpGet]
    public async Task StreamStatus(string operationId)
    {
        await _catalogService.StreamOperationUpdatesAsync(Response, operationId);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

// Legacy request model for backward compatibility
public class CatalogRequest
{
    public string? OperationType { get; set; }
    public System.Text.Json.JsonElement? Parameters { get; set; }
}
