using Microsoft.AspNetCore.Mvc;

namespace WebApp.ViewComponents;

public class HeaderViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}