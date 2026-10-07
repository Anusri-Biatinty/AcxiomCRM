using AcxiomCRM.Data;
using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));  

// ── Identity (password policy + lockout) ─────────────────────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password policyS
    options.Password.RequiredLength          = 8;
    options.Password.RequireDigit            = true;
    options.Password.RequireLowercase        = true;
    options.Password.RequireUppercase        = true;
    options.Password.RequireNonAlphanumeric  = true;

    // Lockout
    options.Lockout.MaxFailedAccessAttempts  = 5;
    options.Lockout.DefaultLockoutTimeSpan   = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers       = true;

    // User
    options.User.RequireUniqueEmail          = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ── Auth cookie ───────────────────────────────────────────────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath          = "/Account/Login";
    options.LogoutPath         = "/Account/Logout";
    options.AccessDeniedPath   = "/Account/AccessDenied";
    options.SlidingExpiration  = true;
    options.ExpireTimeSpan     = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly    = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // use Always in production
});

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddScoped<IAuditService,       AuditService>();
builder.Services.AddScoped<ICustomerService,    CustomerService>();
builder.Services.AddScoped<ILeadService,        LeadService>();
builder.Services.AddScoped<IOpportunityService, OpportunityService>();
builder.Services.AddScoped<IFollowUpService,    FollowUpService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── Seed roles + admin account ────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
