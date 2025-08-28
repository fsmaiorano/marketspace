using Microsoft.AspNetCore.Mvc;
using WebApp.Dtos;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMarketSpaceService service, ILogger<ProductsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetCatalogResponse>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            logger.LogInformation("Fetching products - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            GetCatalogResponse response = await service.GetProductsAsync(page, pageSize, cancellationToken);

            if (response.Products.Count == 0 && page > 1)
            {
                return NoContent();
            }

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Request canceled for page {Page}", page);
            return BadRequest("Request was canceled");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching products for page {Page}", page);
            return StatusCode(500, "Internal server error");
        }
    }
}