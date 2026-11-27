using Optic.Identity.Models.Navigation;

namespace Optic.Identity.Services
{
    public class ApplicationMenuService : IApplicationMenuService
    {
        private readonly IUserAuthenticationService userAuthenticationService;

        public ApplicationMenuService(IUserAuthenticationService userAuthenticationService)
        {
            this.userAuthenticationService = userAuthenticationService;
        }


        public List<ApplicationMenuItem> GetAllMenus()
        {
            return new List<ApplicationMenuItem>();
        }
    }
}
