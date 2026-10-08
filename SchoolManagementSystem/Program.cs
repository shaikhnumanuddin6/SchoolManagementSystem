using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Application.Interfaces;
using SchoolManagementSystem.Application.Services;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// DATABASE
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection connection string was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});


// ============================================================
// APPLICATION SERVICES
// ============================================================

// ------------------------------------------------------------
// Admin Dashboard
// ------------------------------------------------------------

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();


// ------------------------------------------------------------
// Student Management
// ------------------------------------------------------------

builder.Services.AddScoped<
    IStudentService,
    StudentService>();


// ------------------------------------------------------------
// Teacher Management
// ------------------------------------------------------------

builder.Services.AddScoped<
    ITeacherService,
    TeacherService>();


// ------------------------------------------------------------
// Teacher Dashboard
// ------------------------------------------------------------

builder.Services.AddScoped<
    ITeacherDashboardService,
    TeacherDashboardService>();


// ============================================================
// ASP.NET CORE IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // --------------------------------------------------------
        // Password settings
        // --------------------------------------------------------

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;


        // --------------------------------------------------------
        // User settings
        // --------------------------------------------------------

        options.User.RequireUniqueEmail = true;

        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyz" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789-._@";


        // --------------------------------------------------------
        // Account lockout
        // --------------------------------------------------------

        options.Lockout.AllowedForNewUsers = true;

        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);


        // --------------------------------------------------------
        // Sign-in settings
        // --------------------------------------------------------

        options.SignIn.RequireConfirmedAccount = false;

        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// ============================================================
// APPLICATION COOKIE
// ============================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";

    options.LogoutPath = "/Account/Logout";

    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan =
        TimeSpan.FromHours(8);

    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;

    options.Cookie.SameSite =
        SameSiteMode.Lax;

    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;
});


// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();


// ============================================================
// HTTP CLIENT
// ============================================================
//
// Available for future:
// - OpenAI
// - Azure OpenAI
// - Gemini
// - Other AI providers
//
// ============================================================

builder.Services.AddHttpClient();


// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// DATABASE MIGRATION + SEEDING
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // --------------------------------------------------------
        // Get database context
        // --------------------------------------------------------

        var context =
            services.GetRequiredService<ApplicationDbContext>();


        // --------------------------------------------------------
        // Apply pending migrations
        // --------------------------------------------------------

        await context.Database.MigrateAsync();


        // --------------------------------------------------------
        // Seed roles, users and sample data
        // --------------------------------------------------------

        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger =
            services.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            ex,
            "An error occurred while migrating and seeding the database.");

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


// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();


// ============================================================
// STATIC FILES
// ============================================================

app.UseStaticFiles();


// ============================================================
// ROUTING
// ============================================================

app.UseRouting();


// ============================================================
// AUTHENTICATION
// ============================================================

app.UseAuthentication();


// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();


// ============================================================
// MVC ROUTING
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


// ============================================================
// START APPLICATION
// ============================================================

app.Run();