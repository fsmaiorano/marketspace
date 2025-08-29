using Microsoft.AspNetCore.Mvc;
using WebApp.Dtos;

namespace WebApp.ViewComponents;

public class ProductItemViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(CatalogDto product)
    {
        return View(product);
    }
}