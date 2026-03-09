namespace BackendForFrontend.Api.Order.Dtos;

public class GetOrdersByCatalogIdsResponse
{
    public List<GetOrderResponse> Orders { get; set; } = new();
}
