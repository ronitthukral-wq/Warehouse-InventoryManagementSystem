using Inventory.Contracts.Requests.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "StoreManager")]
public class InventoryController : BaseController
{
    // GET: Inventory/AddStock (SM purchases stock into their warehouse)
    public IActionResult AddStock() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStock(AddStockRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var response = await Mediator.Send(request);
        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", response.Message);
        return View(request);
    }

    // GET: Inventory/RequestTransfer (SM initiates transfer to another warehouse)
    public IActionResult RequestTransfer() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestTransfer(CreateTransferRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var response = await Mediator.Send(request);
        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(PendingTransfers));
        }

        ModelState.AddModelError("", response.Message);
        return View(request);
    }

    // GET: Inventory/PendingTransfers (SM inbox to Accept/Reject incoming stock)
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

    // GET: Inventory/History (Audit log of all movements)
    public async Task<IActionResult> History()
    {
        var history = await Mediator.Send(new GetMovementHistoryRequest());
        return View(history);
    }
}