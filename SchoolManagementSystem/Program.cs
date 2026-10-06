using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DATABASE
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection connection string was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IDashboardService, DashboardService>();


// ============================================================
// ASP.NET CORE IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;

        // User settings
        options.User.RequireUniqueEmail = true;

        // Account lockout
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Application Cookie & Authentication Paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});


// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();


var app = builder.Build();

// ============================================================
// DATABASE AUTO-MIGRATION & SEEDING ON STARTUP
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // 1. Auto-migrate database schema on startup
        context.Database.Migrate();

        // 2. Automatically seed default roles, credentials & sample data
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating and seeding the database.");
        throw;
    }
}


// ============================================================
// HTTP PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();


// IMPORTANT:
// Authentication must come BEFORE Authorization.
app.UseAuthentication();
app.UseAuthorization();


// ============================================================
// ROUTING
// Default routing leads to /Account/Login when app starts
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
