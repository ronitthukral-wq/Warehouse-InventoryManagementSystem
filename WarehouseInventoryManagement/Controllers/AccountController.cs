using Microsoft.AspNetCore.Mvc;

namespace WarehouseInventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
