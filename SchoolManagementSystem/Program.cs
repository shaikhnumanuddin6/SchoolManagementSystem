
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

// Dashboard
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Students
builder.Services.AddScoped<IStudentService, StudentService>();

//Teacher
builder.Services.AddScoped<ITeacherService, TeacherService>();

// Future services can be registered here.
// Example:
//
// builder.Services.AddScoped<ITeacherService, TeacherService>();
// builder.Services.AddScoped<IAttendanceService, AttendanceService>();
// builder.Services.AddScoped<IGradeService, GradeService>();
// builder.Services.AddScoped<IAIService, AIService>();


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

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});


// ============================================================
// MVC + REST API
// ============================================================

builder.Services.AddControllersWithViews();


// ============================================================
// HTTP CLIENT
// ============================================================
// Required later for:
// - OpenAI
// - Azure OpenAI
// - Gemini
// - Other external APIs
//
// Keeping HttpClient available now makes the application
// ready for the future AI infrastructure.

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
        var context =
            services.GetRequiredService<ApplicationDbContext>();

        // --------------------------------------------------------
        // Apply pending EF Core migrations
        // --------------------------------------------------------

        await context.Database.MigrateAsync();


        // --------------------------------------------------------
        // Seed:
        // - Roles
        // - Default users
        // - Sample data
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


// ------------------------------------------------------------
// HTTPS
// ------------------------------------------------------------

app.UseHttpsRedirection();


// ------------------------------------------------------------
// Static files
// ------------------------------------------------------------

app.UseStaticFiles();


// ------------------------------------------------------------
// Routing
// ------------------------------------------------------------

app.UseRouting();


// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

app.UseAuthentication();


// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

app.UseAuthorization();


// ============================================================
// MVC ROUTING
// ============================================================

// Application starts at:
//
// /Account/Login
//
// After login, controllers can redirect users according
// to their roles.

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");


// ============================================================
// START APPLICATION
// ============================================================

app.Run();

