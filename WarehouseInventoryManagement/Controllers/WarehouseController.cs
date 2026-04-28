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

        if (response.Success)
        {
            // Store the message for the next request
            TempData["SuccessMessage"] = response.Message;

            // Redirect specifically to the Dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", response.Message);
        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await Mediator.Send(new GetWarehouseByIdRequest { Id = id });

        if (warehouse == null) return NotFound();

        // Map the Response to a Request model to fix the Type Mismatch
        var model = new UpdateWarehouseRequest
        {
            Id = warehouse.Id,
            Name = warehouse.Name,
            Location = warehouse.Location
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateWarehouseRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var response = await Mediator.Send(request);

        if (response.Success)
        {
            TempData["SuccessMessage"] = response.Message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", response.Message);
        return View(request);
    }
}