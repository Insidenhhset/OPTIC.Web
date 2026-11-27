using Optic.Identity.Models.Navigation;

namespace Optic.Identity.Services
{
    public interface IApplicationMenuService
    {

        List<ApplicationMenuItem> GetAllMenus();
    }
}
