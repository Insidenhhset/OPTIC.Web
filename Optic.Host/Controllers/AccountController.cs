
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Optic.Identity.Models;
using Optic.Identity.Services;
using Optic.Identity.Services.Interfaces;
using Optic.Host.Models.Account;

namespace Optic.Host.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserAuthenticationService _authenticationService;
        private readonly ISessionService _sessionService;
        private readonly SessionSettings _sessionSettings;

        public AccountController(
            IUserAuthenticationService authenticationService,
            ISessionService sessionService,
            IOptions<SessionSettings> sessionSettings)
        {
            _authenticationService = authenticationService;
            _sessionService = sessionService;
            _sessionSettings = sessionSettings.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authenticationService.AuthenticateAsync(
                model.Username,
                model.Password);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");

                return View(model);
            }

            var sessionId = await _sessionService.CreateSessionAsync(
                user.Id,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers.UserAgent.ToString());

            var sessionExpiry = DateTimeOffset.UtcNow.AddMinutes(
                _sessionSettings.AbsoluteTimeoutMinutes);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.GivenName,
                    user.FirstName),

                new Claim(
                    ClaimTypes.Surname,
                    user.LastName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim("AvatarUrl", user.AvatarUrl ?? string.Empty),

                new Claim(
                    "optic:session_expires_at",
                    sessionExpiry.ToString("O")),

                new Claim(
                    "optic:session_id",
                    sessionId.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = false,
                    ExpiresUtc = sessionExpiry
                });

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var sessionIdValue = User.FindFirst("optic:session_id")?.Value;

                if (Guid.TryParse(sessionIdValue, out var sessionId))
                    await _sessionService.LogoutAsync(sessionId);
            }
            finally
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
