using Inventory.Contracts.Requests.Products;
using Inventory.Contracts.Requests.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize]
public class ProductController : BaseController
{
    public async Task<IActionResult> Index()
    {
        var products = await Mediator.Send(new GetAllProductsRequest());
        return View(products);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var response = await Mediator.Send(request);

        if (response.Success)
        {
            TempData["SuccessMessage"] = response.Message;
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError("", response.Message);
        return View(request);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await Mediator.Send(new GetProductByIdRequest { Id = id });
        return View(product);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await Mediator.Send(new GetProductByIdRequest { Id = id });
        if (product is null || product.Id == 0)
            return NotFound();

        var model = new UpdateProductRequest
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Description = product.Description,
            LowStockThreshold = product.LowStockThreshold
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProductRequest request)
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LowStockAlerts()
    {
        var items = await Mediator.Send(new GetLowStockReportRequest());
        return View(items);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await Mediator.Send(new DeleteProductRequest { Id = id });
        TempData[response.Success ? "Success" : "Error"] = response.Message;
        return RedirectToAction(nameof(Index));
    }
}
