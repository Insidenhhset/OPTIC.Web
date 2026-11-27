using Optic.Identity.Repositories.Interfaces;
using Optic.Identity.Services.Interfaces;

namespace Optic.Identity.Services
{
    public class PermissionAuthorizationService
        : IPermissionAuthorizationService
    {
        private readonly IPermissionAuthorizationRepository _repository;

        public PermissionAuthorizationService(
            IPermissionAuthorizationRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> HasPermissionAsync(
            int userId,
            string applicationCode,
            string permissionCode)
        {
            return _repository.HasPermissionAsync(
                userId,
                applicationCode,
                permissionCode);
        }
    }
}