using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Data; // For DbInitializer
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Inventory.ServiceLogic; // We'll assume you have a DependencyInjection class here later

var builder = WebApplication.CreateBuilder(args);

// --- 1. DATABASE CONFIGURATION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- 2. IDENTITY CONFIGURATION ---
// We use ApplicationUser and IdentityRole (important for Admin/SM logic)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<InventoryDbContext>()
.AddDefaultTokenProviders();

// --- 3. MVC & REFRESH SERVICES ---
builder.Services.AddControllersWithViews();

// --- 4. MEDIATR & AUTOMAPPER (The Logic Brain) ---
// This tells MediatR to look for Handlers in your ServiceLogic project
var serviceLogicAssembly = AppDomain.CurrentDomain.Load("Inventory.ServiceLogic");
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(serviceLogicAssembly);
    // cfg.AddOpenBehavior(typeof(ValidationBehavior<,>)); // We'll wire this in the next lesson
});

builder.Services.AddAutoMapper(cfg => {
    if (serviceLogicAssembly != null)
    {
        cfg.AddMaps(serviceLogicAssembly);
    }
});
// --- 5. MIDDLEWARE PIPELINE ---
var app = builder.Build();

// --- 6. SEED THE DATABASE (Senior Practice) ---
// This runs every time the app starts to ensure the Admin user exists
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<InventoryDbContext>();

        // Apply pending migrations automatically (Optional, but handy for dev)
        await context.Database.MigrateAsync();

        // Run our seed logic
        await DbInitializer.SeedData(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database seeding.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// IMPORTANT: Authentication MUST come before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();