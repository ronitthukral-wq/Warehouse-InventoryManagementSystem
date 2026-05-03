using Inventory.Contracts.Requests.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "StoreManager")]
public class InventoryController : BaseController
{
    public async Task<IActionResult> AddStock()
    {
        await PopulateProductDropdownAsync();
        return View(new AddStockRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
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

    public async Task<IActionResult> RequestTransfer()
    {
        await PopulateProductDropdownAsync();
        await PopulateOtherWarehousesDropdownAsync();
        return View(new CreateTransferRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
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

    public async Task<IActionResult> PendingTransfers()
    {
        var transfers = await Mediator.Send(new GetPendingTransfersRequest());
        return View(transfers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RespondToTransfer(RespondToTransferRequest request)
    {
        var response = await Mediator.Send(request);
        TempData[response.Success ? "Success" : "Error"] = response.Message;
        return RedirectToAction(nameof(PendingTransfers));
    }

    public async Task<IActionResult> MyInventory()
    {
        var stock = await Mediator.Send(new GetStockByWarehouseRequest());
        return View(stock);
    }

    public async Task<IActionResult> History()
    {
        var history = await Mediator.Send(new GetMovementHistoryRequest());
        return View(history);
    }

    // ── helpers now go through MediatR ──────────────────────────────
    private async Task PopulateProductDropdownAsync()
    {
        var items = await Mediator.Send(new GetProductDropdownRequest());
        ViewBag.Products = new SelectList(items, "Value", "Display");
    }

    private async Task PopulateOtherWarehousesDropdownAsync()
    {
        var items = await Mediator.Send(new GetOtherWarehousesDropdownRequest());
        ViewBag.Warehouses = new SelectList(items, "Value", "Display");
    }
}