using Inventory.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Inventory.Data;

public static class DbInitializer
{
    public static async Task SeedData(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Seed Roles
        string[] roles = { "Admin", "StoreManager" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // 2. Seed Admin User
        var adminEmail = "admin@wms.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedBy = "System"
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123"); // Use a strong password!
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}