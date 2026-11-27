using Microsoft.AspNetCore.Mvc;

namespace Optic.Host.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
