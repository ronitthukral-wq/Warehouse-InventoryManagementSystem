using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.ViewComponents;

public class DeleteConfirmModalViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string modalId,
        string formId,
        string title,
        string entityLabel,
        string nameElementId)
    {
        ViewBag.ModalId = modalId;
        ViewBag.FormId = formId;
        ViewBag.Title = title;
        ViewBag.EntityLabel = entityLabel;
        ViewBag.NameElementId = nameElementId;
        return View();
    }
}