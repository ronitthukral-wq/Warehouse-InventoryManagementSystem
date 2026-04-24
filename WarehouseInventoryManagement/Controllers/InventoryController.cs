using Microsoft.AspNetCore.Mvc;

namespace WarehouseInventoryManagement.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
