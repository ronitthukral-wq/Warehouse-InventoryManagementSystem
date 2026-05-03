using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.ViewComponents;

public class HeroBannerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string title,
        string subtitle,
        string? primaryButtonText = null,
        string? primaryButtonUrl = null,
        string? secondaryButtonText = null,
        string? secondaryButtonUrl = null)
    {
        ViewBag.Title = title;
        ViewBag.Subtitle = subtitle;
        ViewBag.PrimaryButtonText = primaryButtonText;
        ViewBag.PrimaryButtonUrl = primaryButtonUrl;
        ViewBag.SecondaryButtonText = secondaryButtonText;
        ViewBag.SecondaryButtonUrl = secondaryButtonUrl;
        return View();
    }
}