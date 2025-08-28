using Microsoft.AspNetCore.Mvc;
using WebApp.Dtos;
using WebApp.Services;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IMarketSpaceService service, ILogger<ProductsController> logger) : ControllerBase
{
    /// <summary>
    /// Endpoint otimizado para scroll infinito - retorna produtos paginados de forma assíncrona
    /// </summary>
    /// <param name="page">Número da página (default: 1)</param>
    /// <param name="pageSize">Tamanho da página (default: 20)</param>
    /// <param name="cancellationToken">Token de cancelamento para operações assíncronas</param>
    /// <returns>Resposta com produtos paginados</returns>
    [HttpGet]
    public async Task<ActionResult<GetCatalogResponse>> GetProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validação básica dos parâmetros
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 20;

            logger.LogInformation("Fetching products - Page: {Page}, PageSize: {PageSize}", page, pageSize);

            // Usa CancellationToken para permitir cancelamento se o usuário navegar para outra página
            GetCatalogResponse response = await service.GetProductsAsync(page, pageSize, cancellationToken);

            // Retorna 204 No Content se não há mais dados
            if (response.Products.Count == 0 && page > 1)
            {
                return NoContent();
            }

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            // Log de operação cancelada (normal quando usuário navega rapidamente)
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
