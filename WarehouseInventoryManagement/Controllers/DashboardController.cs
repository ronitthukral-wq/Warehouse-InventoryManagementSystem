using Inventory.Contracts.Requests.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    public async Task<IActionResult> Index()
    {
        // The Handler will use IHttpContextAccessor to check the user's role/warehouseId
        var metrics = await Mediator.Send(new GetDashboardRequest());
        return View(metrics);
    }
}