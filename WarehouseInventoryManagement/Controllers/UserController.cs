using Inventory.Contracts.Requests.Users;
using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : BaseController
{
    public async Task<IActionResult> Index()
    {
        var users = await Mediator.Send(new GetAllUsersRequest());
        return View(users);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateWarehousesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStoreManagerRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateWarehousesAsync(request.WarehouseId);
            return View(request);
        }

        var response = await Mediator.Send(request);

        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", response.Message);
        await PopulateWarehousesAsync(request.WarehouseId);
        return View(request);
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var user = await Mediator.Send(new GetUserByIdRequest { Id = id });
        if (user is null) return NotFound();

        var model = new UpdateStoreManagerRequest
        {
            Id = user.Id,
            Email = user.Email,
            // Try to recover the WarehouseId from the displayed name
            WarehouseId = await ResolveWarehouseIdAsync(user.AssignedWarehouseName)
        };

        await PopulateWarehousesAsync(model.WarehouseId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateStoreManagerRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateWarehousesAsync(request.WarehouseId);
            return View(request);
        }

        var response = await Mediator.Send(request);

        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", response.Message);
        await PopulateWarehousesAsync(request.WarehouseId);
        return View(request);
    }

    // ----- Helpers -----

    private async Task PopulateWarehousesAsync(int? selectedId = null)
    {
        var warehouses = await Mediator.Send(new GetAllWarehousesRequest());
        ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name", selectedId);
    }

    private async Task<int?> ResolveWarehouseIdAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var warehouses = await Mediator.Send(new GetAllWarehousesRequest());
        return warehouses.FirstOrDefault(w =>
            string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase))?.Id;
    }
}
