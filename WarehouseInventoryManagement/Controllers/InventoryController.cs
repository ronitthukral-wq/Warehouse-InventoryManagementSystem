using Inventory.Contracts.Requests.Inventory;
using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Requests.Warehouses;
using Inventory.ServiceLogic.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory.Web.Controllers;

[Authorize]
public class InventoryController : BaseController
{
    private readonly ICurrentUserService _currentUser;

    public InventoryController(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    // ---------- Store Manager: My Inventory ----------

    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> MyStock()
    {
        var stock = await Mediator.Send(new GetMyStockRequest());
        ViewBag.WarehouseId = await _currentUser.GetWarehouseIdAsync();
        return View(stock);
    }

    // ---------- Store Manager: Add Stock ----------

    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> AddStock()
    {
        await PopulateProductsAsync();
        return View(new AddStockRequest());
    }

    [HttpPost]
    [Authorize(Roles = "StoreManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStock(AddStockRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductsAsync(request.ProductId);
            return View(request);
        }

        var response = await Mediator.Send(request);
        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(MyStock));
        }

        ModelState.AddModelError("", response.Message);
        await PopulateProductsAsync(request.ProductId);
        return View(request);
    }

    // ---------- Store Manager: Request Transfer ----------

    [Authorize(Roles = "StoreManager")]
    public async Task<IActionResult> RequestTransfer()
    {
        await PopulateProductsAsync();
        await PopulateOtherWarehousesAsync();
        return View(new CreateTransferRequest());
    }

    [HttpPost]
    [Authorize(Roles = "StoreManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestTransfer(CreateTransferRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateProductsAsync(request.ProductId);
            await PopulateOtherWarehousesAsync(request.ToWarehouseId);
            return View(request);
        }

        var response = await Mediator.Send(request);
        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(PendingTransfers));
        }

        ModelState.AddModelError("", response.Message);
        await PopulateProductsAsync(request.ProductId);
        await PopulateOtherWarehousesAsync(request.ToWarehouseId);
        return View(request);
    }

    // ---------- Pending Transfers (incoming inbox) ----------

    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> PendingTransfers()
    {
        var transfers = await Mediator.Send(new GetPendingTransfersRequest());
        return View(transfers);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,StoreManager")]
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

    // ---------- History (warehouse-scoped for SM, global for Admin) ----------

    [Authorize(Roles = "Admin,StoreManager")]
    public async Task<IActionResult> History()
    {
        var history = await Mediator.Send(new GetMovementHistoryRequest());
        return View(history);
    }

    // ---------- Helpers ----------

    private async Task PopulateProductsAsync(int? selectedId = null)
    {
        var products = await Mediator.Send(new GetAllProductsRequest());
        ViewBag.Products = new SelectList(products, "Id", "Name", selectedId);
    }

    private async Task PopulateOtherWarehousesAsync(int? selectedId = null)
    {
        var myWarehouseId = await _currentUser.GetWarehouseIdAsync();
        var warehouses = await Mediator.Send(new GetAllWarehousesRequest());
        var filtered = warehouses.Where(w => w.Id != myWarehouseId).ToList();
        ViewBag.Warehouses = new SelectList(filtered, "Id", "Name", selectedId);
    }
}
