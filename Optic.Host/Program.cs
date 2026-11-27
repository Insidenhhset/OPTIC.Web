using Microsoft.AspNetCore.Authentication.Cookies;
using Optic.Audit.DI;
using Optic.Host.Middleware;
using Optic.Identity.DI;
using Optic.Notification.DI;

var builder = WebApplication.CreateBuilder(args);

var identityModulePath = Path.GetFullPath(
    Path.Combine(
        builder.Environment.ContentRootPath,
        "..",
        "Optic.Identity"));

var identityConfiguration = new ConfigurationBuilder()
    .SetBasePath(identityModulePath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// MVC
builder.Services.AddControllersWithViews()
    .AddApplicationPart(
        typeof(Optic.Notification.Controllers.NotificationController).Assembly)
    .AddApplicationPart(
        typeof(Optic.Audit.Controllers.AuditController).Assembly)
    .AddApplicationPart(typeof(Optic.Identity.Controllers.IdentityUsersController).Assembly);

// Module registrations
builder.Services.AddIdentityServices(identityConfiguration);
builder.Services.AddNotificationServices();
builder.Services.RegisterAuditServices();

// Session settings
var absoluteTimeoutMinutes = identityConfiguration.GetValue<int>("SessionSettings:AbsoluteTimeoutMinutes", 60);

// Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "OPTIC.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromMinutes(absoluteTimeoutMinutes);

        options.SlidingExpiration = false;

        // AJAX authentication handling
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                context.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseExceptionHandler();
app.UseRouting();
app.UseMiddleware<TimeZoneMiddleware>();
app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
