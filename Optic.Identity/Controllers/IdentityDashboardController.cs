using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Optic.Identity.Models.Navigation;
using Optic.Identity.Services.Interfaces;
using System.Security.Claims;

namespace Optic.Identity.Controllers
{
    [Authorize]
    public class IdentityDashboardController : Controller
    {
        private readonly IApplicationNavigationService _navigationService;

        public IdentityDashboardController(IApplicationNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdValue, out var userId))
                return Forbid();

            var menus = await _navigationService.GetAccessibleMenusAsync(userId, 2);

            var dashboardMenus = GetDashboardMenus(menus);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_IdentityDashboardContent",dashboardMenus);
            }

            return View(dashboardMenus);
        }

        private static List<ApplicationMenuItem> GetDashboardMenus(
            IEnumerable<ApplicationMenuItem> menus)
        {
            var result = new List<ApplicationMenuItem>();

            foreach (var menu in menus)
            {
                if (menu.ShowOnDashboard)
                    result.Add(menu);

                result.AddRange(GetDashboardMenus(menu.Children));
            }

            return result;
        }
    }
}