using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Requests.Warehouses;
using Inventory.Contracts.Responses;
using Inventory.Data.Context;
using Inventory.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "StoreManager")]
public class InventoryController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InventoryDbContext _db;

    public InventoryController(UserManager<ApplicationUser> userManager, InventoryDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    // GET: Inventory/AddStock
    public async Task<IActionResult> AddStock()
    {
        await PopulateProductDropdownAsync();
        return View(new AddStockRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStock(AddStockRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductDropdownAsync();
            return View(request);
        }

        var response = await Mediator.Send(request);
        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError(string.Empty, response.Message);
        await PopulateProductDropdownAsync();
        return View(request);
    }

    // GET: Inventory/RequestTransfer
    public async Task<IActionResult> RequestTransfer()
    {
        await PopulateProductDropdownAsync();
        await PopulateOtherWarehousesDropdownAsync();
        return View(new CreateTransferRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestTransfer(CreateTransferRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductDropdownAsync();
            await PopulateOtherWarehousesDropdownAsync();
            return View(request);
        }

        var response = await Mediator.Send(request);
        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(PendingTransfers));
        }

        ModelState.AddModelError(string.Empty, response.Message);
        await PopulateProductDropdownAsync();
        await PopulateOtherWarehousesDropdownAsync();
        return View(request);
    }

    // GET: Inventory/PendingTransfers (incoming + outgoing for the current SM)
    public async Task<IActionResult> PendingTransfers()
    {
        var transfers = await Mediator.Send(new GetPendingTransfersRequest());
        return View(transfers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToTransfer(RespondToTransferRequest request)
    {
        var response = await Mediator.Send(request);
        if (response.Success)
            TempData["Success"] = response.Message;
        else
            TempData["Error"] = response.Message;

        return RedirectToAction(nameof(PendingTransfers));
    }

    // GET: Inventory/MyInventory
    // Shows the Store Manager every product currently in their warehouse, with
    // a clickable link through to the product detail page.
    public async Task<IActionResult> MyInventory()
    {
        var stock = await Mediator.Send(new GetStockByWarehouseRequest());
        return View(stock);
    }

    // GET: Inventory/History
    public async Task<IActionResult> History()
    {
        var history = await Mediator.Send(new GetMovementHistoryRequest());
        return View(history);
    }

    // ----------------- private helpers -----------------

    private async Task PopulateProductDropdownAsync()
    {
        var products = await _db.Products
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, Display = p.Name + " (" + p.SKU + ")" })
            .ToListAsync();

        ViewBag.Products = new SelectList(products, "Id", "Display");
    }

    // Lists every warehouse EXCEPT the current Store Manager's own.
    // Used as the "source warehouse" picker on the Request Transfer form.
    private async Task PopulateOtherWarehousesDropdownAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        var myWarehouseId = user?.WarehouseId ?? 0;

        var warehouses = await _db.Warehouses
            .Where(w => w.Id != myWarehouseId)
            .OrderBy(w => w.Name)
            .Select(w => new { w.Id, Display = w.Name + " - " + w.Location })
            .ToListAsync();

        ViewBag.Warehouses = new SelectList(warehouses, "Id", "Display");
    }
}
