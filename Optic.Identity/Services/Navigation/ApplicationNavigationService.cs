using Optic.Identity.Models.Navigation;
using Optic.Identity.Repositories;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Services.Navigation
{
    public class ApplicationNavigationService
        : IApplicationNavigationService
    {
        private readonly IApplicationNavigationRepository _repository;

        public ApplicationNavigationService(
            IApplicationNavigationRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ApplicationNavigationItem>>
            GetAccessibleApplicationsAsync(int userId)
        {
            if (userId <= 0)
                return new List<ApplicationNavigationItem>();

            return await _repository.GetAccessibleApplicationsAsync(userId);
        }

        public async Task<List<UserApplicationRole>> GetUserRolesAsync(
    int userId)
        {
            if (userId <= 0)
                return new List<UserApplicationRole>();

            return await _repository.GetUserRolesAsync(userId);
        }

        public async Task<List<ApplicationMenuItem>>
            GetAccessibleMenusAsync(int userId, int applicationId)
        {
            if (userId <= 0 || applicationId <= 0)
                return new List<ApplicationMenuItem>();

            return await _repository.GetAccessibleMenusAsync(
                userId,
                applicationId);
        }
    }
}
