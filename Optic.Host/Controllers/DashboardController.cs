
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optic.Host.Models.Dashboard;
using Optic.Identity.Services.Interfaces;

namespace Optic.Host.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IApplicationNavigationService _navigationService;

        public DashboardController(
            IApplicationNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Challenge();

            var applications =
                await _navigationService.GetAccessibleApplicationsAsync(
                    userId);

            var model = new DashboardViewModel
            {
                Applications = applications
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_DashboardContent", model);

            return View(model);
        }
    }
}
