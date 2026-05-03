using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.ViewComponents;

public class AlertToastViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var success = ViewContext.TempData["Success"]?.ToString();
        var error = ViewContext.TempData["Error"]?.ToString();

        ViewBag.Success = success;
        ViewBag.Error = error;

        return View();
    }
}