using System;

namespace WebApp.Dtos;

public class GetCatalogResponse
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public long Count { get; set; }
    public List<CatalogDto> Products { get; set; }
}
