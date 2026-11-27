using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optic.Notification.Services;

namespace Optic.Notification.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotifcationService _notificationService;

        public NotificationController(INotifcationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            _notificationService.sum();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_NotificationContent");
            }

            return View();
        }
    }
}