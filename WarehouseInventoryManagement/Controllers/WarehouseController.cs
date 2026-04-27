using Inventory.Contracts.Requests.Warehouses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin")]
public class WarehouseController : BaseController
{
    public async Task<IActionResult> Index()
    {
        var warehouses = await Mediator.Send(new GetAllWarehousesRequest());
        return View(warehouses);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWarehouseRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var response = await Mediator.Send(request);
        if (response.Success) return RedirectToAction(nameof(Index));

        ModelState.AddModelError("", response.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await Mediator.Send(new GetWarehouseByIdRequest { Id = id });
        return View(warehouse);
    }
}