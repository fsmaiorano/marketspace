using Microsoft.AspNetCore.Mvc;
using WebApp.Dtos;

namespace WebApp.ViewComponents;

public class ProductListViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(List<CatalogDto> products)
    {
        return View(products);
    }
}