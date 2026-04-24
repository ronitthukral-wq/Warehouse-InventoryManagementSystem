using Microsoft.AspNetCore.Mvc;

namespace WarehouseInventoryManagement.Controllers
{
    public class WarehouseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
