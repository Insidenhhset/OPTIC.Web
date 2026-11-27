
using Optic.Identity.Models.Navigation;

namespace Optic.Identity.Services.Interfaces
{
    public interface IApplicationNavigationService
    {
        Task<List<ApplicationNavigationItem>> GetAccessibleApplicationsAsync(
            int userId);

        Task<List<ApplicationMenuItem>> GetAccessibleMenusAsync(
            int userId,
            int applicationId);

        Task<List<UserApplicationRole>> GetUserRolesAsync(int userId);
    }
}
