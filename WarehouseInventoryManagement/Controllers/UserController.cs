using Inventory.Contracts.Requests.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

[Authorize(Roles = "Admin")]
public class UserController : BaseController
{
    public async Task<IActionResult> Index()
    {
        var users = await Mediator.Send(new GetAllUsersRequest());
        return View(users);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateStoreManagerRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        var response = await Mediator.Send(request);

        if (response.Success)
        {
            TempData["Success"] = response.Message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", response.Message);
        return View(request);
    }
}