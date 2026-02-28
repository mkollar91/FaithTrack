// =============================================================
// FaithTrack — Program.cs
// Application entry point. Configures services, middleware,
// and routing for the ASP.NET Core MVC (.NET 8) application.
// Implements the layered architecture defined in the
// Architecture Plan (Milestone 3): Presentation, Application,
// and Data Access layers wired via Dependency Injection.
//
// Author  : Matthew Kollar
// Course  : CST-451 Capstone Project
// School  : Grand Canyon University
// Version : Draft 1.0  |  Sprint 1
// =============================================================

using FaithTrack.Data;
using FaithTrack.Models;
using FaithTrack.Repositories;
using FaithTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database Context ──────────────────────────────────────
// Registers FaithTrackDbContext with SQL Server Express/LocalDB.
// Connection string is read from appsettings.json.
// Satisfies the Data Access Layer in the Architecture Plan
// Physical Deployment Diagram (Fig. 4).
builder.Services.AddDbContext<FaithTrackDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. ASP.NET Core Identity ─────────────────────────────────
// Provides secure user registration, login, password hashing
// (PBKDF2), and session management as specified in the
// Architecture Plan NFR — Authentication & Authorization (p.26).
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Password complexity rules (NFR — Password Security, p.26)
    options.Password.RequireDigit           = true;
    options.Password.RequireLowercase       = true;
    options.Password.RequireUppercase       = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength         = 8;

    // Email confirmation disabled for local development
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<FaithTrackDbContext>();

// ── 3. Cookie Authentication Settings ───────────────────────
// Session cookies use HttpOnly and HTTPS as required by the
// Architecture Plan NFR — Session Management (p.26).
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath         = "/Account/Login";
    options.LogoutPath        = "/Account/Logout";
    options.AccessDeniedPath  = "/Account/AccessDenied";
    options.Cookie.HttpOnly   = true;
    options.Cookie.SameSite   = SameSiteMode.Lax;
    options.ExpireTimeSpan    = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── 4. Repositories — Data Access Layer ─────────────────────
// Scoped lifetime matches EF Core DbContext lifetime.
// Implements the Repository interfaces defined in the
// UML Class Diagram (Fig. 15, Data Access Layer).
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// ── 5. Services — Application / Business Logic Layer ────────
// Implements the Service interfaces defined in the
// UML Class Diagram (Fig. 15, Application Layer).
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// ── 6. MVC ───────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── 7. Logging ───────────────────────────────────────────────
// Built-in ASP.NET Core logging as specified in the
// Architecture Plan Operational Support Design (p.27).
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ════════════════════════════════════════════════════════════
var app = builder.Build();
// ════════════════════════════════════════════════════════════

// ── 8. HTTP Pipeline ─────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    // Generic error page prevents leaking system info (NFR p.27)
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication MUST precede Authorization in the pipeline
app.UseAuthentication();
app.UseAuthorization();

// ── 9. Routes ────────────────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ── 10. Seed Default Data ────────────────────────────────────
// Creates default categories on first run so the Create
// Resource dropdown is not empty.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

app.Run();
