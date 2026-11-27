
using Optic.Identity.Models.Navigation;

namespace Optic.Host.Models.Dashboard
{
    public class DashboardViewModel
    {
        public List<ApplicationNavigationItem> Applications { get; set; } = new();
    }
}
