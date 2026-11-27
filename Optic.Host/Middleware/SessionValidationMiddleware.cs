
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Optic.Identity.Services;
using Optic.Identity.Services.Interfaces;

namespace Optic.Host.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ISessionService sessionService)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var sessionIdValue =
                context.User.FindFirst("optic:session_id")?.Value;

            if (!Guid.TryParse(sessionIdValue, out var sessionId))
            {
                await RejectSessionAsync(context);
                return;
            }

            var isValid = await sessionService.ValidateSessionAsync(sessionId);

            if (!isValid)
            {
                await RejectSessionAsync(context);
                return;
            }

            await _next(context);
        }

        private static async Task RejectSessionAsync(HttpContext context)
        {
            await context.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            var isAjaxRequest =
                context.Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest";

            if (isAjaxRequest)
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;

                return;
            }

            var loginUrl = "/Account/Login?reason=session-expired";

            context.Response.Redirect(loginUrl);
        }
    }
}
