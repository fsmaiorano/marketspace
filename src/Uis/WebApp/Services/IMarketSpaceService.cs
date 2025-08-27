namespace WebApp.Services;

public interface IMarketSpaceService
{
    Task<List<string>> GetProductsAsync();
}