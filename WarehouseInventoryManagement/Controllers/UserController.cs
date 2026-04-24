using Microsoft.AspNetCore.Mvc;

namespace WarehouseInventoryManagement.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
