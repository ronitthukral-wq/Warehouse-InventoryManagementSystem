using Inventory.Data.Context;
using Inventory.Models.Entities;
using Inventory.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Inventory.ServiceLogic;
using Serilog;

// --- 0. SERILOG BOOTSTRAP LOGGER ---
// Captures errors that happen before configuration is loaded.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Warehouse Inventory Management web host");

    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog (reads "Serilog" section from appsettings.json)
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // --- 1. DATABASE CONFIGURATION ---
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<InventoryDbContext>(options =>
        options.UseSqlServer(connectionString));

    // --- 2. IDENTITY CONFIGURATION ---
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddEntityFrameworkStores<InventoryDbContext>()
    .AddDefaultTokenProviders();

    // CRITICAL: Configure Cookie to prevent 404 on Unauthorized access
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

    // --- 3. MVC SERVICES ---
    builder.Services.AddControllersWithViews();

    // Allow Razor to resolve views in BOTH singular and plural folder names
    // (e.g. ProductController will look in /Views/Product/ AND /Views/Products/).
    // This keeps the project resilient to historical folder naming.
    builder.Services.Configure<RazorViewEngineOptions>(options =>
    {
        options.ViewLocationFormats.Add("/Views/{1}s/{0}" + RazorViewEngine.ViewExtension);
        options.ViewLocationFormats.Add("/Views/{1}es/{0}" + RazorViewEngine.ViewExtension);
    });

    builder.Services.AddServiceLogic();

    // --- 5. MIDDLEWARE PIPELINE ---
    var app = builder.Build();

    // --- 6. SEED THE DATABASE ---
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var context = services.GetRequiredService<InventoryDbContext>();

            await context.Database.MigrateAsync();
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

    // Per-request structured HTTP logging (method, path, status, elapsed ms)
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    // IMPORTANT: Authentication MUST come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Web host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}