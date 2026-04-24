using Microsoft.AspNetCore.Mvc;

namespace WarehouseInventoryManagement.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
