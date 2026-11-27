
using Microsoft.AspNetCore.Mvc;
using Optic.Audit.Models;
using Optic.Audit.Services;

namespace Optic.Audit.Controllers
{
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            await _auditService.LogActivityAsync(new AuditLog
            {
                UserId = "EMP001",
                UserName = "Admin",
                ModuleName = "Audit",
                Action = "View",
                Description = "Opened Audit Dashboard",
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            });

            var activities =
                await _auditService.GetRecentActivitiesAsync(10);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_IndexContent", activities);
            }

            return View(activities);
        }
    }
}
