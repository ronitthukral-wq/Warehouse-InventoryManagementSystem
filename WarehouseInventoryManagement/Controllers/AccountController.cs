using Inventory.Contracts.Requests.Users;
using Inventory.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        // Check if user is already authenticated
        if (User.Identity?.IsAuthenticated ?? false)
        {
            // Redirect to a main page, e.g., Products, since you have no HomeController
            return RedirectToAction("Index", "Dashboard");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        // Identity-based login logic
        var result = await _signInManager.PasswordSignInAsync(
            request.Email,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Redirect to your primary functional area
            return RedirectToAction("Index", "Dashboard");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}