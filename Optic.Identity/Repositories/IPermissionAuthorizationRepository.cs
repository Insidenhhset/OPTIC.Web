namespace Optic.Identity.Repositories.Interfaces
{
    public interface IPermissionAuthorizationRepository
    {
        Task<bool> HasPermissionAsync(
            int userId,
            string applicationCode,
            string permissionCode);
    }
}