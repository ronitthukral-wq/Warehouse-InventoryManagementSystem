using Inventory.Contracts.Requests.Products;
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
        var response = await Mediator.Send(request);
        if (response.Success) return RedirectToAction(nameof(Index));
        return View(request);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await Mediator.Send(new GetProductByIdRequest { Id = id });
        return View(product);
    }
}